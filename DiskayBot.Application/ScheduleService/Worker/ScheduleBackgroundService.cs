using System.Diagnostics;
using DiskayBot.API.Clients;
using DiskayBot.API.Contracts;
using DiskayBot.API.Contracts.Schedule;
using DiskayBot.API.Interfaces;
using DiskayBot.API.Modules;
using DiskayBot.Bot.ScheduleService.Events;
using DiskayBot.Bot.ScheduleService.Options;
using DiskayBot.Redis.Abstractions;
using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DiskayBot.Bot.ScheduleService.Worker;

public class ScheduleBackgroundService : BackgroundService {
    private readonly IScheduleClient  _scheduleClient;
    private readonly IRedisController _redis;
    private readonly IMediator _mediator;
    private readonly UserClient _userClient;
    private readonly RoomScheduleState _roomSchedule;
    private readonly ScheduleServiceOptions _options;
    private readonly ILogger<ScheduleBackgroundService> _logger;

    private IReadOnlyList<string> _groups = Array.Empty<string>();

    public ScheduleBackgroundService(IMediator mediator, IScheduleClient scheduleClient, IRedisController redis, UserClient userClient, RoomScheduleState roomSchedule, IOptions<ScheduleServiceOptions> options, ILogger<ScheduleBackgroundService> logger) {
        _mediator =  mediator;
        _scheduleClient = scheduleClient;
        _redis = redis;
        _userClient = userClient;
        _roomSchedule = roomSchedule;
        _options = options.Value;
        _logger = logger;
    }

    private async Task<bool> RefreshGroups() {
        try {
            var groups = await _userClient.GetAllGroups();
            if (groups is { Count: > 0 }) {
                _groups = groups.Select(group => group.name).ToList();
                return true;
            }
            else {
                _logger.LogWarning("DiskayMemory вернул пустой список групп, оставляю предыдущий ({Count})", _groups.Count);
            }
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Не удалось обновить список групп, оставляю предыдущий ({Count})", _groups.Count);
        }
        return false;
    }

    private async Task UpdateSchedule() {
        var startedAt = Stopwatch.GetTimestamp();
        var changedDays = 0;
        var failedRequests = 0;

        var complete = await RefreshGroups();
        if (!complete) {
            _roomSchedule.Publish(null);
        }
        var today = DateOnly.FromDateTime(DateTime.Now);
        var currentWeek = TimeHelper.GetWeekPeriod(today);
        var todaySchedules = new List<DaySchedule>(_groups.Count);

        // Следующая неделя нужна, чтобы в субботу показывать понедельник
        TimePeriod[] weeks = [
            currentWeek,
            TimeHelper.GetWeekPeriod(currentWeek.End.AddDays(1))
        ];

        foreach (var week in weeks) {
            var isCurrentWeek = week.Start == currentWeek.Start;
            foreach (var group in _groups) {
                try {
                    var freshWeekSchedule = await _scheduleClient.GetWeekSchedule(group, week);
                    changedDays += await UpdateWeek(group, week, freshWeekSchedule);
                    if (isCurrentWeek && complete && freshWeekSchedule != null) {
                        foreach (var day in freshWeekSchedule.Schedule) {
                            if (day.date == today) {
                                todaySchedules.Add(day);
                            }
                        }
                    }
                }
                catch (Exception ex) {
                    if (isCurrentWeek) {
                        complete = false;
                        _roomSchedule.Publish(null);
                    }
                    failedRequests++;
                    _logger.LogError(ex, "Ошибка обновления расписания для группы '{Group}' на неделю с {WeekStart:dd.MM.yyyy}",
                        group, week.Start);
                }
            }
            if (isCurrentWeek && complete) {
                _roomSchedule.Publish(new RoomScheduleSnapshot(today, todaySchedules.ToArray()));
            }
        }

        // Пустой цикл повторяется каждые updateTimeout секунд, на Information он только засоряет лог
        var level = changedDays > 0 || failedRequests > 0 ? LogLevel.Information : LogLevel.Debug;
        _logger.Log(level,
            "Обновление расписания завершено за {ElapsedMs:F0} мс: групп {Groups}, изменилось дней {ChangedDays}, ошибок {FailedRequests}",
            Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds, _groups.Count, changedDays, failedRequests);
    }

    private async Task<int> UpdateWeek(string group, TimePeriod week, GroupWeekSchedule? freshWeekSchedule) {
        var changedDays = 0;
        if (freshWeekSchedule != null) {
            foreach (var freshDaySchedule in freshWeekSchedule.Schedule) {
                var pastScheduleIsActual = await _redis.CheckScheduleEquals(freshDaySchedule);
                if (pastScheduleIsActual == false) {
                    await _mediator.Publish(new ScheduleUpdatedEvent(freshDaySchedule));
                    await _redis.SaveSchedule(freshDaySchedule);
                    changedDays++;
                }
                else {
                    await _redis.SetScheduleDefaultExpire(freshDaySchedule);
                }
            }
        }

        // Пустой ответ колледжа — тоже успешная загрузка: значит, пар на неделе нет.
        // Помечаем после сохранения дней, чтобы неделя не считалась загруженной без своих дней
        await _redis.MarkWeekLoaded(group, week.Start);
        return changedDays;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        _logger.LogInformation("Обновление расписания запущено, интервал {UpdateTimeout} с", _options.updateTimeout);
        var timer = new PeriodicTimer(TimeSpan.FromSeconds(_options.updateTimeout));
        do {
            await UpdateSchedule();
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
