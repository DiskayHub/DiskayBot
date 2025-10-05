using DiskayBot.API.Contracts;
using DiskayBot.Services.ScheduleService.Data;
using Microsoft.Extensions.Logging;

namespace DiskayBot.Services.ScheduleService.Components;

public class ScheduleAnalyser {
    private readonly ScheduleService _service;
    private readonly ILogger<ScheduleAnalyser> _logger;
    private WeekSchedule? _lastSchedule;
    public event Action NewWeekScheduleAppear;
    public ScheduleAnalyser(ScheduleService scheduleService, ILogger<ScheduleAnalyser> logger) {
        _service = scheduleService;
        _logger = logger;
        _lastSchedule = null;
    }
    private void Analyse(WeekSchedule updatedWeekSchedule) {
        _logger.LogInformation("Анализирую расписание..");
        if (_lastSchedule != null) {
            if (Equals(_lastSchedule.WeekPeriod, updatedWeekSchedule.WeekPeriod)) {
                foreach ((string group, List<DaySchedule> scheduleList) in _lastSchedule.GroupsSchedule) {
                    if (!updatedWeekSchedule.GroupsSchedule.TryGetValue(group, out var otherList)) {
                        _logger.LogInformation($"ИЗМЕНЕНИЕ: НЕ НАЙДЕНА ГРУППА {group}");
                    }

                    if (otherList != null && !scheduleList.SequenceEqual(otherList))
                        _logger.LogInformation($"РАСПИСАНИЕ ДЛЯ ГРУППЫ {group} НЕ СООТВЕТСТВУЕТ НОВОМУ РАСПИСАНИЮ");
                }
            }
            else {
                _logger.LogInformation("Обнаружено новое недельное расписание!");
                NewWeekScheduleAppear.Invoke();
            }
        }
        _logger.LogInformation("Анализ завершён");
    }

    private void InitSchedule() {
        _lastSchedule = _service.Schedule;
    }

    public void Listen() {
        _service.OnFirstScheduleAppear += InitSchedule;
        _service.OnScheduleUpdated += Analyse;
    }
}