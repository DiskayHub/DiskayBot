using DiskayBot.Bot.Events;
using DiskayBot.Bot.Interfaces;

namespace DiskayBot.Bot.Abstractions;

public abstract class EventProcessor : IEventProcessor {
    public readonly string Name;

    public EventProcessor(string name) {
        Name = name;
    }
    
    public abstract Task HandleAsync(UserEvent evt, CancellationToken cancellationToken);
}