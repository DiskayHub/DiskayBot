using DiskayBot.API.Contracts;

namespace DiskayBot.Services.ScheduleService.Interfaces;

public interface IScheduleController {
    public DaySchedule? GetActualSchedule(string groupName);
    public List<DaySchedule>? GetWeekSchedule(string groupName);
}