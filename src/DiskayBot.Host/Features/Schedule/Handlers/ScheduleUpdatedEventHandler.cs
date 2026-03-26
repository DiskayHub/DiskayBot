using DiskayBot.Host.Features.Schedule.Events;
using MediatR;

namespace DiskayBot.Host.Features.Schedule.Handlers;

public class ScheduleUpdatedEventHandler : INotificationHandler<ScheduleUpdatedEvent> {
    public Task Handle(ScheduleUpdatedEvent notification, CancellationToken cancellationToken) {
        Console.WriteLine("ScheduleUpdatedEventHandler");
        return Task.CompletedTask;
    }
}