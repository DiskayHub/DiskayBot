using DiskayBot.Bot.Attributes;
using DiskayBot.Bot.Bot.Messages;
using DiskayBot.Bot.DTOs;
using DiskayBot.Bot.Interfaces;
using DiskayBot.Bot.ScheduleService;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace DiskayBot.Bot.Bot.Commands;

[CommandName("/space", AccessLevel.User)]
public sealed class SpaceCommand(RoomScheduleState state, TimeProvider clock) : IBaseCommand {
    public async Task ExecuteAsync(BotContext ctx, CancellationToken token) {
        var localNow = clock.GetLocalNow().DateTime;
        var snapshot = state.Read();
        var today = DateOnly.FromDateTime(localNow);
        var now = TimeOnly.FromDateTime(localNow);
        var rooms = snapshot?.Date == today ? RoomAvailability.Find(snapshot.Days, now) : null;
        await ctx.Bot.SendMessage(ctx.Event.Chat, MessageBuilder.ShowFreeRooms(today, now, rooms), ParseMode.Html,
            cancellationToken: token);
    }
}
