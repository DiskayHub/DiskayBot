using DiskayBot.API.Contracts;

namespace DiskayBot.Bot.ScheduleService;

public interface IScheduleController {
    Task<DaySchedule?> GetActualSchedule(string groupName);
    Task<DaySchedule?> GetNextSchedule(string groupName, DateOnly date);
    Task<DaySchedule?> GetPreviousSchedule(string groupName, DateOnly date);
}