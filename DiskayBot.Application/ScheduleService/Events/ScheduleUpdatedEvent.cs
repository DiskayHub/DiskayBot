using DiskayBot.API.Contracts;
using MediatR;

namespace DiskayBot.Bot.ScheduleService.Events;

public record ScheduleUpdatedEvent(
    DaySchedule DaySchedule
) : INotification;