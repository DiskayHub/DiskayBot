using System.Globalization;
using System.Text;
using DiskayBot.API.Contracts;
using DiskayBot.Bot.Attributes;
using DiskayBot.Bot.DTOs;
using DiskayBot.Bot.Interfaces;
using DiskayBot.Bot.ScheduleService;
using Telegram.Bot;

namespace DiskayBot.Bot.Bot.Commands;

[CommandName("/space", AccessLevel.User)]
public sealed class SpaceCommand(RoomScheduleState state, TimeProvider clock) : IBaseCommand {
    public async Task ExecuteAsync(BotContext ctx, CancellationToken token) {
        var localNow = clock.GetLocalNow().DateTime;
        var snapshot = state.Read();
        var text = snapshot == null || snapshot.Date != DateOnly.FromDateTime(localNow)
            ? "Расписание всех групп на сегодня пока недоступно. Попробуйте позже."
            : FormatRooms(snapshot.Days, TimeOnly.FromDateTime(localNow));
        await ctx.Bot.SendMessage(ctx.Event.Chat, text, cancellationToken: token);
    }

    private static string FormatRooms(IReadOnlyList<DaySchedule> days, TimeOnly now) {
        var rooms = RoomAvailability.Find(days, now);
        if (rooms.Count == 0) {
            return "По расписанию сейчас нет свободных кабинетов.";
        }

        var text = new StringBuilder("Свободные кабинеты по расписанию:");
        foreach (var room in rooms) {
            var endTicks = room.NextStart?.Ticks ?? TimeSpan.TicksPerDay;
            var minutes = (endTicks - now.Ticks) / TimeSpan.TicksPerMinute;
            text.Append('\n').Append(room.Name).Append(" — до ");
            if (room.NextStart is { } start) {
                text.Append(CultureInfo.InvariantCulture, $"{start:HH:mm}");
            }
            else {
                text.Append("конца дня");
            }
            text.Append(" (");
            if (minutes == 0) {
                text.Append("меньше 1 мин");
            }
            else if (minutes < 60) {
                text.Append(minutes).Append(" мин");
            }
            else {
                text.Append(minutes / 60).Append(" ч ").Append(minutes % 60).Append(" мин");
            }
            text.Append(')');
        }
        return text.ToString();
    }
}
