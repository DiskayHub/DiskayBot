namespace DiskayBot.API.Contracts.Schedule;

public record SubGroupResponse(
    string name,
    string description,
    string roomName,
    string subGroup
);

public record ItemResponse(
    string name,
    string description,
    string? room_name,
    TimeOnly startTime,
    TimeOnly endTime,
    List<SubGroupResponse>? subGroups
);

public record DayScheduleResponse(
    DateOnly date,
    string mainGroup,
    List<ItemResponse> items
);