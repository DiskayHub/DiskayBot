namespace DiskayBot.API.Contracts;

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
) {
    public virtual bool Equals(DayItem? other) {
        if (other is  null) return false;
        
        bool subGroupsEqual = 
            (subGroups is null && other.subGroups is null) ||
            (subGroups is not null && other.subGroups is not null && subGroups.SequenceEqual(other.subGroups));

        return
            name == other.name &&
            description == other.description &&
            room_name == other.room_name &&
            startTime.Equals(other.startTime) &&
            endTime.Equals(other.endTime) &&
            subGroupsEqual;
    }
};

public record DaySchedule(
    DateOnly date,
    string mainGroup,
    List<DayItem> items
) {
    public virtual bool Equals(DaySchedule? other) {
        return 
            other is not null && 
            date == other.date &&
            mainGroup == other.mainGroup &&
            items.SequenceEqual(other.items);
    }
};