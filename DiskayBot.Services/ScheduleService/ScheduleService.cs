using DiskayBot.API.Clients;
using DiskayBot.API.Contracts;
using Microsoft.Extensions.Logging;

namespace DiskayBot.Services.ScheduleService;

public class ScheduleService {
    private readonly ScheduleClient _client;
    private readonly List<string> _allGroups;
    private readonly ILogger<ScheduleService> _logger;
    private Dictionary<string, List<DaySchedule>> _groupDaySchedules;
    
    public event Action<Dictionary<string, List<DaySchedule>>> OnAllSchedulesUpdated;
    
    public ScheduleService(ScheduleClient client, ILogger<ScheduleService> logger) {
        _client = client;
        _allGroups = [
            "ИТ25-11", "ИТ25-12", "ИТ25-13", "ИТ25-14",
            "ИТ24-11", "ИТ24-12", "ИТ24-13", "ИТ24-14",
            "ИТ23-11", "ИТ23-12", "ИТ23-13",
            "ИТ22-11", "ИТ22-12"
        ];
        _logger = logger;
        _groupDaySchedules = new Dictionary<string, List<DaySchedule>>();
    }

    private async Task UpdateSchedules() {
        _logger.LogInformation("Обновление данных о расписании...");
        foreach (var group in _allGroups) {
            var schedule = await _client.GetCurrentScheduleWeek(group);
            if (schedule != null) {
                _groupDaySchedules[group] = schedule;
            }
        }
        _logger.LogInformation($"Обновление завершено. Всего групп: {_groupDaySchedules.Count}");
        OnAllSchedulesUpdated?.Invoke(_groupDaySchedules);
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
        _groupDaySchedules.TryGetValue(groupName, out var schedule);
        return schedule;
    }

    public async Task Run(TimeSpan delay, CancellationToken? token = default) {
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