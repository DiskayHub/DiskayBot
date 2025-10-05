using DiskayBot.API.Clients;
using DiskayBot.API.Contracts;
using DiskayBot.API.Interfaces;
using DiskayBot.API.Modules;
using DiskayBot.Services.ScheduleService.Data;
using DiskayBot.Services.ScheduleService.Interfaces;
using Microsoft.Extensions.Logging;

namespace DiskayBot.Services.ScheduleService;

public class ScheduleService : IScheduleServiceEvents {
    private readonly IScheduleClient _client;
    private readonly List<string> _allGroups;
    private readonly ILogger<ScheduleService> _logger;
    public readonly ScheduleAnalyser Analyser;
    public WeekSchedule Schedule {get; set; }
    public event Action<WeekSchedule> OnScheduleUpdated;
    
    public ScheduleService(IScheduleClient client, ILogger<ScheduleService> logger, ILoggerFactory loggerFactory) {
        _client = client;
        _allGroups = [
            "ИТ25-11", "ИТ25-12", "ИТ25-13", "ИТ25-14",
            "ИТ24-11", "ИТ24-12", "ИТ24-13", "ИТ24-14",
            "ИТ23-11", "ИТ23-12", "ИТ23-13",
            "ИТ22-11", "ИТ22-12"
        ];
        _logger = logger;
        Schedule = new WeekSchedule();
        Analyser = new ScheduleAnalyser(this, loggerFactory.CreateLogger<ScheduleAnalyser>());
    }

    private async Task UpdateSchedules() {
        _logger.LogInformation("Обновление данных о расписании...");
        var previosSchedule = Schedule;
        foreach (var group in _allGroups) {
            var schedule = await _client.GetActualScheduleWeek(group);
            if (schedule != null) {
                Schedule.WeekPeriod = schedule.WeekPeriod;
                Schedule.GroupsSchedule[group] = schedule.Schedule;
            }
        }
        _logger.LogInformation($"Обновление завершено. Всего групп: {Schedule.GroupsSchedule.Count}");

        var scheduleUpdateEvent = new WeekSchedule(
            weekPeriod: TimeHelper.GetActualWeekPeriod(),
            groupsSchedule: Schedule.GroupsSchedule
        );
        
        OnScheduleUpdated.Invoke(scheduleUpdateEvent);
    }

    public DaySchedule? GetActualSchedule(string groupName) {
        _logger.LogInformation("Запрос на актуальное расписание");
        var dateTimeNow = DateTime.Now;
        
        var weekSchedule = GetSchedule(groupName);
        if (weekSchedule != null) {
            foreach (var schedule in weekSchedule) {
                if (schedule.date.Day == dateTimeNow.Day && schedule.items[^1].endTime > TimeOnly.FromDateTime(DateTime.Now)) {
                    _logger.LogInformation(schedule.items[^1].endTime.ToString("hh:mm:ss"));
                    return schedule;
                }
                if (schedule.date > DateOnly.FromDateTime(dateTimeNow)) {
                    return schedule;
                }
            }
        }
        return null;
    }

    public List<DaySchedule>? GetSchedule(string groupName) {
        _logger.LogInformation($"Получаю расписание для группы: {groupName}...");
        Schedule.GroupsSchedule.TryGetValue(groupName, out var schedule);
        return schedule;
    }

    public async Task Run(TimeSpan delay, CancellationToken? token = default) {
        Analyser.Listen();
        _logger.LogInformation("Запуск сервиса ScheduleService");
        
        var timer = new PeriodicTimer(delay);
        try {
            await UpdateSchedules();
            if (await timer.WaitForNextTickAsync()) {
                await UpdateSchedules();
            }
        }
        catch (HttpRequestException ex) {
            _logger.LogError(ex, "Ошибка при отправке запроса к Schedule API!");
        }
    }
}