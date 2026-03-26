using DiskayBot.Infrastructure.Contracts.Schedule;

namespace DiskayBot.Infrastructure.Interfaces;

public interface IScheduleClient {
    public Task<GroupWeekSchedule?> GetActualScheduleWeek(string groupName);
    public Task<GroupWeekSchedule?> GetCurrentWeekSchedule(string groupName);
}