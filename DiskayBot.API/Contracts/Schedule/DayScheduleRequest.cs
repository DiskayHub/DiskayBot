namespace DiskayBot.API.Contracts.Schedule;

public record DayScheduleRequest(
    DateOnly date,
    string group_name
);