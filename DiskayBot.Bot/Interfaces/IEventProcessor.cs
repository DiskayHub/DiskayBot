using DiskayBot.Bot.Abstractions;
using DiskayBot.Bot.Events;

namespace DiskayBot.Bot.Interfaces;

public interface IEventProcessor {
    public Task HandleAsync(UserEvent evt, CancellationToken cancellationToken);
}