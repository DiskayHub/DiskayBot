using DiskayBot.API.Contracts;

namespace DiskayBot.Bot.ScheduleService;

public sealed record RoomScheduleSnapshot(DateOnly Date, IReadOnlyList<DaySchedule> Days);

public sealed class RoomScheduleState {
    private RoomScheduleSnapshot? _snapshot;

    public RoomScheduleSnapshot? Read() => Volatile.Read(ref _snapshot);

    public void Publish(RoomScheduleSnapshot? snapshot) => Volatile.Write(ref _snapshot, snapshot);
}
