using DiskayBot.Infrastructure.Modules;

namespace DiskayBot.Infrastructure.Contracts.Schedule;

public record GroupWeekSchedule(
    TimePeriod WeekPeriod,
    List<DaySchedule> Schedule
);