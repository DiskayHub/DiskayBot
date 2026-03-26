using DiskayBot.Infrastructure.Contracts;

namespace DiskayBot.Host.Features.Schedule;

public interface IScheduleController {
    Task<DaySchedule?> GetActualSchedule(string groupName);
    Task<DaySchedule?> GetNextSchedule(string groupName, DateOnly date);
    Task<DaySchedule?> GetPreviousSchedule(string groupName, DateOnly date);
}