using System.Diagnostics;
using DiskayBot.API.Clients;
using DiskayBot.API.Interfaces;
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
    private readonly ScheduleServiceOptions _options;
    private readonly ILogger<ScheduleBackgroundService> _logger;

    private IReadOnlyList<string> _groups = Array.Empty<string>();

    public ScheduleBackgroundService(IMediator mediator, IScheduleClient scheduleClient, IRedisController redis, UserClient userClient, IOptions<ScheduleServiceOptions> options, ILogger<ScheduleBackgroundService> logger) {
        _mediator =  mediator;
        _scheduleClient = scheduleClient;
        _redis = redis;
        _userClient = userClient;
        _options = options.Value;
        _logger = logger;
    }

    private async Task RefreshGroups() {
        try {
            var groups = await _userClient.GetAllGroups();
            if (groups is { Count: > 0 }) {
                _groups = groups.Select(group => group.name).ToList();
            }
            else {
                _logger.LogWarning("DiskayMemory вернул пустой список групп, оставляю предыдущий ({Count})", _groups.Count);
            }
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Не удалось обновить список групп, оставляю предыдущий ({Count})", _groups.Count);
        }
    }

    private async Task UpdateSchedule() {
        var startedAt = Stopwatch.GetTimestamp();
        var changedDays = 0;
        var failedGroups = 0;

        await RefreshGroups();

        foreach (var group in _groups) {
            try {
                var freshWeekSchedule = await _scheduleClient.GetCurrentWeekSchedule(group);
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
            }
            catch (Exception ex) {
                failedGroups++;
                _logger.LogError(ex, "Ошибка обновления расписания для группы '{Group}'", group);
            }
        }

        // Пустой цикл повторяется каждые updateTimeout секунд, на Information он только засоряет лог
        var level = changedDays > 0 || failedGroups > 0 ? LogLevel.Information : LogLevel.Debug;
        _logger.Log(level,
            "Обновление расписания завершено за {ElapsedMs:F0} мс: групп {Groups}, изменилось дней {ChangedDays}, ошибок {FailedGroups}",
            Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds, _groups.Count, changedDays, failedGroups);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        _logger.LogInformation("Обновление расписания запущено, интервал {UpdateTimeout} с", _options.updateTimeout);
        var timer = new PeriodicTimer(TimeSpan.FromSeconds(_options.updateTimeout));
        do {
            await UpdateSchedule();
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
