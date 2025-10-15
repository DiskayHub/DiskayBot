using DiskayBot.API.Contracts;
using DiskayBot.API.Interfaces;
using DiskayBot.Services.ScheduleService.Interfaces;
using Microsoft.Extensions.Logging;

namespace DiskayBot.Services.ScheduleService.Components;

/// <summary>
/// Класс для взаимодействия с сервисом расписания
/// </summary>
public class ScheduleController : IScheduleController {
    private readonly IScheduleService _scheduleService;
    private readonly ILogger<ScheduleController> _logger;
    
    public ScheduleController(IScheduleService scheduleService, ILogger<ScheduleController> logger) {
        _scheduleService = scheduleService;
        _logger = logger;
    }
    
    public DaySchedule? GetActualSchedule(string groupName) {
        _logger.LogInformation("Запрос на актуальное расписание");
        var dateTimeNow = DateTime.Now;
        
        var weekSchedule = _scheduleService.GetWeekSchedule(groupName);
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

    public List<DaySchedule>? GetWeekSchedule(string groupName) {
        return _scheduleService.GetWeekSchedule(groupName);
    }
}