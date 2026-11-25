using DiskayBot.API.Contracts;

namespace DiskayBot.Bot.ScheduleService;

public sealed record FreeRoom(string Name, TimeOnly? NextStart, IReadOnlyList<string> NextGroups);

public static class RoomAvailability {
    private static readonly string[] Rooms = [
        "1-3", "2-1", "2-3", "2-4", "2-5", "3-0", "3-1", "3-2", "3-3", "3-4", "3-5", "3-e",
        "403", "404", "405", "406", "410"
    ];
    private static readonly Dictionary<string, int> RoomIndices = Rooms
        .Select((name, index) => (name, index))
        .ToDictionary(room => room.name, room => room.index, StringComparer.OrdinalIgnoreCase);

    public static IReadOnlyList<FreeRoom> Find(IReadOnlyList<DaySchedule> days, TimeOnly now) {
        var lookup = RoomIndices.GetAlternateLookup<ReadOnlySpan<char>>();
        var occupancy = new (bool Busy, TimeOnly? NextStart, List<string>? NextGroups)[Rooms.Length];
        foreach (var day in days) {
            foreach (var item in day.items) {
                if (item.endTime <= item.startTime || item.endTime <= now) {
                    continue;
                }
                AddRoom(item.room_name, item.startTime, day.mainGroup);
                if (item.subGroups != null) {
                    foreach (var subgroup in item.subGroups) {
                        AddRoom(subgroup.roomName, item.startTime, day.mainGroup);
                    }
                }
            }
        }

        var free = new List<FreeRoom>(Rooms.Length);
        for (var index = 0; index < Rooms.Length; index++) {
            ref var slot = ref occupancy[index];
            if (!slot.Busy) {
                slot.NextGroups?.Sort(StringComparer.Ordinal);
                free.Add(new FreeRoom(Rooms[index], slot.NextStart,
                    (IReadOnlyList<string>?)slot.NextGroups ?? Array.Empty<string>()));
            }
        }
        free.Sort(static (left, right) => {
            var byStart = (right.NextStart?.Ticks ?? TimeSpan.TicksPerDay)
                .CompareTo(left.NextStart?.Ticks ?? TimeSpan.TicksPerDay);
            return byStart != 0 ? byStart : StringComparer.Ordinal.Compare(left.Name, right.Name);
        });
        return free;

        void AddRoom(string? label, TimeOnly start, string group) {
            var room = label.AsSpan().Trim();
            if (room.Equals("3-1/2", StringComparison.OrdinalIgnoreCase)) {
                Register("3-1", start, group);
                Register("3-2", start, group);
                return;
            }
            if (room.Equals("каб 1-3", StringComparison.OrdinalIgnoreCase) ||
                room.Equals("№3 1 этаж", StringComparison.OrdinalIgnoreCase)) {
                room = "1-3";
            }
            Register(room, start, group);
        }

        void Register(ReadOnlySpan<char> room, TimeOnly start, string group) {
            if (!lookup.TryGetValue(room, out var index)) {
                return;
            }
            ref var slot = ref occupancy[index];
            if (slot.Busy) {
                return;
            }
            if (start <= now) {
                slot.Busy = true;
                return;
            }
            if (slot.NextStart is not { } nextStart || start < nextStart) {
                slot.NextStart = start;
                var groups = slot.NextGroups ??= new List<string>(1);
                groups.Clear();
                groups.Add(group);
            }
            else if (start == nextStart && !slot.NextGroups!.Contains(group)) {
                slot.NextGroups.Add(group);
            }
        }
    }
}
