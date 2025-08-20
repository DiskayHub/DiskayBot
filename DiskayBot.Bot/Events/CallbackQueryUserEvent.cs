using DiskayBot.Bot.Abstractions;

namespace DiskayBot.Bot.Events;

public class CallbackQueryUserEvent : UserEvent {
    public readonly string Name;
    public readonly string? Query;

    public CallbackQueryUserEvent(UpdateInfo info, string callBackQuery) : base(info) {
        var parts = callBackQuery.Split('=');
        
        if (parts.Length == 2){
            Name = parts[0];
            Query = parts[1];
        }
        else {
            Name = callBackQuery;
        }
    }

    public override string GetContent() {
        return Name;
    }
}