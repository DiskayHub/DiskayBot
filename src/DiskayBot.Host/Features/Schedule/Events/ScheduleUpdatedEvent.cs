using DiskayBot.Infrastructure.Contracts;
using MediatR;

namespace DiskayBot.Host.Features.Schedule.Events;

public record ScheduleUpdatedEvent(
    DaySchedule DaySchedule
) : INotification;