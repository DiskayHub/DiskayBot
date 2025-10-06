using DiskayBot.Services.ScheduleService.Events;

namespace DiskayBot.Services.ScheduleService.Interfaces;

public interface IScheduleAnalyser {
    event Func<Task> NewWeekScheduleAppear;
    void Analyse(UpdateScheduleEvent updatedEvent);
    void UpdateAnalysis(UpdateScheduleEvent updatedEvent);
    void Listen();
}