using DiskayBot.Bot.Abstractions;

namespace DiskayBot.Bot.Events;

public class CallbackQueryUserEvent : UserEvent {
    public readonly string Id;
    public readonly string Name;
    public readonly string? Query;
    public readonly List<string>? QueryArgs;

    public CallbackQueryUserEvent(UpdateInfo info, string callBackQuery, string id) : base(info) {
        Id = id;
        var parts = callBackQuery.Split('=');
        
        if (parts.Length >= 2){
            Name = parts[0];
            Query = parts[1];
            if (parts.Length > 2) {
                QueryArgs = parts.Skip(2).ToList();
            }
        }
        else {
            Name = callBackQuery;
        }
    }

    public override string GetContent() {
        return Name;
    }
}