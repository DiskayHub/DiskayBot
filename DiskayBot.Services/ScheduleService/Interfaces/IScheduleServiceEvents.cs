using DiskayBot.Services.ScheduleService.Data;
using DiskayBot.Services.ScheduleService.Events;

namespace DiskayBot.Services.ScheduleService.Interfaces;

public interface IScheduleServiceEvents {
    public event Action<UpdateScheduleEvent> OnScheduleUpdated;
}