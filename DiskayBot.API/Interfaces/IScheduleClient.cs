using DiskayBot.API.Contracts.Schedule;

namespace DiskayBot.API.Interfaces;

public interface IScheduleClient {
    public Task<GroupWeekSchedule?> GetActualScheduleWeek(string groupName);
}