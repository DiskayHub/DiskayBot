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

        Assert.Collection(free.Where(room => room.NextStart != null),
            room => AssertNext(room, "1-3"),
            room => AssertNext(room, "3-e"));
        Assert.All(new[] { "404", "405", "410" }, name => {
            var room = Assert.Single(free, room => room.Name == name);
            Assert.Null(room.NextStart);
            Assert.Empty(room.NextGroups);
        });
        Assert.DoesNotContain(free, room => room.Name.Equals("Online", StringComparison.OrdinalIgnoreCase));

        static void AssertNext(FreeRoom room, string name) {
            Assert.Equal(name, room.Name);
            Assert.Equal(new TimeOnly(14, 0), room.NextStart);
            Assert.Equal("ИТ26-11", Assert.Single(room.NextGroups));
        }
    }

    private static DayItem Lesson(string? room, TimeOnly start, TimeOnly end, params string[] subrooms) =>
        new("Пара", "", room, start, end,
            subrooms.Length == 0 ? null : subrooms.Select(label => new SubGroupItem("Пара", "", label, "1")).ToList());
}
