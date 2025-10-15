using DiskayBot.API.Contracts;
using DiskayBot.Services.ScheduleService.Data;

namespace DiskayBot.Services.ScheduleService.Interfaces;

public interface IScheduleService {
    public Task Run(TimeSpan delay, CancellationToken? token = default);
    public List<DaySchedule>? GetWeekSchedule(string groupName);
    public WeekSchedule Schedule { get; }
}