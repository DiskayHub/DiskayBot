using DiskayBot.API.Contracts;
using DiskayBot.Services.ScheduleService.Events;
using DiskayBot.Services.ScheduleService.Interfaces;
using Microsoft.Extensions.Logging;

namespace DiskayBot.Services.ScheduleService.Components;

public class ScheduleAnalyser : IScheduleAnalyser {
    private readonly IScheduleServiceEvents _events;
    private readonly ILogger<ScheduleAnalyser> _logger;
    public event Func<Task> NewWeekScheduleAppear;
    public event Func<Task> ScheduleChanged;
    public ScheduleAnalyser(IScheduleServiceEvents scheduleEvents, ILogger<ScheduleAnalyser> logger) {
        _events = scheduleEvents;
        _logger = logger;
    }

    public void Analyse(UpdateScheduleEvent updatedEvent) {
        _logger.LogInformation("Анализирую расписание..");
        UpdateAnalysis(updatedEvent);
        _logger.LogInformation("Анализ завершён");
    }

    public void UpdateAnalysis(UpdateScheduleEvent updatedEvent) {
        _logger.LogDebug("Проверка соответствия с прошлым расписанием..");
        var previosWeekSchedule = updatedEvent.previosWeekSchedule;
        var currentWeekSchedule = updatedEvent.currentWeekSchedule;
        
        if (previosWeekSchedule != null && previosWeekSchedule.GroupsSchedule.Count != 0) {
            if (Equals(previosWeekSchedule.WeekPeriod, currentWeekSchedule.WeekPeriod)) {
                foreach ((string group, List<DaySchedule> scheduleList) in previosWeekSchedule.GroupsSchedule) {
                    if (!currentWeekSchedule.GroupsSchedule.TryGetValue(group, out var otherList)) {
                        _logger.LogInformation($"ИЗМЕНЕНИЕ: НЕ НАЙДЕНА ГРУППА {group}");
                    }
                    if (otherList != null && !scheduleList.SequenceEqual(otherList))
                        ScheduleChanged.Invoke();
                }
            }
            else {
                _logger.LogInformation("Обнаружено новое недельное расписание!");
                NewWeekScheduleAppear.Invoke();
            }
        }
        else {
            _logger.LogDebug("Нет исходных данных, анализ сравнения невозможен");
        }
    }

    public void Listen() {
        _logger.LogInformation("Подключаю обработчики для анализа расписания...");
        _events.OnScheduleUpdated += Analyse;
        _logger.LogInformation("ScheduleAnalyser слушает сервис расписания");
    }
}