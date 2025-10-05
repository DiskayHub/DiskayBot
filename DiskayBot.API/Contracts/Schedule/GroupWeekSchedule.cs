using DiskayBot.API.Modules;

namespace DiskayBot.API.Contracts.Schedule;

public record GroupWeekSchedule(
    TimePeriod WeekPeriod,
    List<DaySchedule> Schedule
);