using DiskayBot.API.Contracts;
using DiskayBot.Services.ScheduleService.Data;
using DiskayBot.Services.ScheduleService.Interfaces;
using Microsoft.Extensions.Logging;

namespace DiskayBot.Services.ScheduleService;

public class ScheduleAnalyser {
    private readonly IScheduleServiceEvents _events;
    private readonly ILogger<ScheduleAnalyser> _logger;
    private WeekSchedule? _lastSchedule;
    public event Func<Task> NewWeekScheduleAppear;
    public ScheduleAnalyser(IScheduleServiceEvents scheduleEvents, ILogger<ScheduleAnalyser> logger) {
        _events = scheduleEvents;
        _logger = logger;
    }
    private void Analyse(WeekSchedule updatedWeekSchedule) {
        _logger.LogInformation("Анализирую расписание..");
        UpdateAnalysis(updatedWeekSchedule);
        _logger.LogInformation("Анализ завершён");
    }
    private void UpdateAnalysis(WeekSchedule updatedWeekSchedule) {
        _logger.LogInformation("Проверка соответствия с прошлым расписанием..");
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
        else {
            _logger.LogInformation("Нет исходных данных, анализ сравнения невозможен");
            _lastSchedule = updatedWeekSchedule;
        }
    }

    public void Listen() {
        _logger.LogInformation("Подключаю обработчики для анализа расписания...");
        _events.OnScheduleUpdated += Analyse;
    }
}