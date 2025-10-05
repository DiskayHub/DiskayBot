using DiskayBot.Services.ScheduleService.Data;

namespace DiskayBot.Services.ScheduleService.Interfaces;

public interface IScheduleServiceEvents {
    public event Action<WeekSchedule> OnScheduleUpdated;
}