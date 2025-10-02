using DiskayBot.API.Contracts;
using Microsoft.Extensions.Logging;

namespace DiskayBot.Services.ScheduleService.Components;

public class ScheduleAnalyser {
    private readonly ScheduleService _service;
    private readonly ILogger<ScheduleAnalyser> _logger;
    public event Action OnScheduleChanged;
    public ScheduleAnalyser(ScheduleService scheduleService, ILogger<ScheduleAnalyser> logger) {
        _service = scheduleService;
        _logger = logger;
    }
    private void Analyse(Dictionary<string, List<DaySchedule>> schedules) {
        _logger.LogInformation("Analysing schedules..");
    }

    public void Listen() {
        _service.OnAllSchedulesUpdated += Analyse;
    }
}