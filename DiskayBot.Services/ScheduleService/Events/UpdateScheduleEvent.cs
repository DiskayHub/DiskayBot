using DiskayBot.Services.ScheduleService.Data;

namespace DiskayBot.Services.ScheduleService.Events;

public record UpdateScheduleEvent(
    WeekSchedule? previosWeekSchedule,
    WeekSchedule currentWeekSchedule
);