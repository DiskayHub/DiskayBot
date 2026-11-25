using DiskayBot.API.Contracts;

namespace DiskayBot.Bot.ScheduleService;

public sealed record FreeRoom(string Name, TimeOnly? NextStart);

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
        var occupancy = new (bool Busy, TimeOnly? NextStart)[Rooms.Length];
        foreach (var day in days) {
            foreach (var item in day.items) {
                if (item.endTime <= item.startTime || item.endTime <= now) {
                    continue;
                }
                AddRoom(item.room_name, item.startTime);
                if (item.subGroups != null) {
                    foreach (var subgroup in item.subGroups) {
                        AddRoom(subgroup.roomName, item.startTime);
                    }
                }
            }
        }

        var free = new List<FreeRoom>(Rooms.Length);
        for (var index = 0; index < Rooms.Length; index++) {
            if (!occupancy[index].Busy) {
                free.Add(new FreeRoom(Rooms[index], occupancy[index].NextStart));
            }
        }
        free.Sort(static (left, right) => {
            var byStart = (right.NextStart?.Ticks ?? TimeSpan.TicksPerDay)
                .CompareTo(left.NextStart?.Ticks ?? TimeSpan.TicksPerDay);
            return byStart != 0 ? byStart : StringComparer.Ordinal.Compare(left.Name, right.Name);
        });
        return free;

        void AddRoom(string? label, TimeOnly start) {
            var room = label.AsSpan().Trim();
            if (room.Equals("3-1/2", StringComparison.OrdinalIgnoreCase)) {
                Register("3-1", start);
                Register("3-2", start);
                return;
            }
            if (room.Equals("каб 1-3", StringComparison.OrdinalIgnoreCase) ||
                room.Equals("№3 1 этаж", StringComparison.OrdinalIgnoreCase)) {
                room = "1-3";
            }
            Register(room, start);
        }

        void Register(ReadOnlySpan<char> room, TimeOnly start) {
            if (!lookup.TryGetValue(room, out var index)) {
                return;
            }
            ref var slot = ref occupancy[index];
            if (start <= now) {
                slot.Busy = true;
            }
            else if (!slot.Busy && (slot.NextStart == null || start < slot.NextStart)) {
                slot.NextStart = start;
            }
        }
    }
}
