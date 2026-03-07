using DiskayBot.Bot.ScheduleService.Events;
using MediatR;

namespace DiskayBot.Bot.ScheduleService.Handlers;

public class ScheduleUpdatedEventHandler : INotificationHandler<ScheduleUpdatedEvent> {
    public Task Handle(ScheduleUpdatedEvent notification, CancellationToken cancellationToken) {
        Console.WriteLine("ScheduleUpdatedEventHandler");
        return Task.CompletedTask;
    }
}