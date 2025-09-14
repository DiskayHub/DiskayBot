namespace DiskayBot.API.Contracts.Schedule;

public record SubGroupItem(
    string name,
    string description,
    string roomName,
    string subGroup
);

public record DayItem(
    string name,
    string description,
    string? room_name,
    TimeOnly startTime,
    TimeOnly endTime,
    List<SubGroupItem>? subGroups
);

public record DaySchedule(
    DateOnly date,
    string mainGroup,
    List<DayItem> items
);