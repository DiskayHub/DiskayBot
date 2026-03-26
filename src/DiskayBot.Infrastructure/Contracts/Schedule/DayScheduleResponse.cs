namespace DiskayBot.Infrastructure.Contracts.Schedule;

public record ApiSubGroup(
    string SClID,
    string SGrID,
    string SGCaID,
    string STopic,
    string STitle
);
public record ApiItem(
    string ClID,
    string Day,
    string group,
    string topic,
    string start,
    string end,
    string room,
    string color,
    string title,
    List<ApiSubGroup>? SubGroup
);

public record ApiScheduleResponse( 
    List<ApiItem> Items
);