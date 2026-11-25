using DiskayBot.API.Contracts;
using DiskayBot.Bot.ScheduleService;

namespace DiskayBot.Tests;

public partial class SpaceCommandTests {
    [Theory]
    [InlineData("  КАБ 1-3 ")]
    [InlineData(" №3 1 ЭТАЖ ")]
    public void Availability_handles_aliases_and_ignores_invalid_intervals_and_unknown_rooms(string alias) {
        var now = new TimeOnly(11, 30);
        var free = RoomAvailability.Find([new DaySchedule(new DateOnly(2026, 11, 25), "ИТ26-11", [
            Lesson(" Online ", new(14, 0), new(15, 0), alias),
            Lesson(" 3-E ", new(14, 0), new(15, 0)),
            Lesson("404", new(13, 0), new(12, 0)),
            Lesson("405", new(13, 0), new(13, 0)),
            Lesson("410", new(11, 0), now)
        ])], now);

        Assert.Equal(new[] { new FreeRoom("1-3", new(14, 0)), new FreeRoom("3-e", new(14, 0)) },
            free.Where(room => room.NextStart != null));
        Assert.All(new[] { "404", "405", "410" }, name => Assert.Contains(new FreeRoom(name, null), free));
        Assert.DoesNotContain(free, room => room.Name.Equals("Online", StringComparison.OrdinalIgnoreCase));
    }

    private static DayItem Lesson(string? room, TimeOnly start, TimeOnly end, params string[] subrooms) =>
        new("Пара", "", room, start, end,
            subrooms.Length == 0 ? null : subrooms.Select(label => new SubGroupItem("Пара", "", label, "1")).ToList());
}
