using DiskayBot.Bot.Attributes;
using DiskayBot.Bot.Bot.Exeptions;
using DiskayBot.Bot.Bot.Messages;
using DiskayBot.Bot.DTOs;
using DiskayBot.Bot.Events;
using DiskayBot.Bot.Interfaces;
using DiskayBot.Bot.ScheduleService;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DiskayBot.Bot.Bot.Commands;

[CommandName("/space", AccessLevel.User)]
[CallbackName(UpdateCallback, AccessLevel.User)]
public sealed class SpaceCommand(RoomScheduleState state, TimeProvider clock) : IBaseCommand {
    private const string UpdateCallback = "updateSpace";
    private static readonly InlineKeyboardMarkup UpdateKeyboard =
        new(InlineKeyboardButton.WithCallbackData("Обновить 💫", UpdateCallback));

    public async Task ExecuteAsync(BotContext ctx, CancellationToken token) {
        var localNow = clock.GetLocalNow().DateTime;
        var snapshot = state.Read();
        var today = DateOnly.FromDateTime(localNow);
        var now = TimeOnly.FromDateTime(localNow);
        var rooms = snapshot?.Date == today ? RoomAvailability.Find(snapshot.Days, now) : null;
        var text = MessageBuilder.ShowFreeRooms(today, rooms);
        if (ctx.Event is CallbackQueryUserEvent callback) {
            await ctx.Bot.AnswerCallbackQuery(callback.Id, cancellationToken: token);
            try {
                await ctx.Bot.EditMessageText(callback.Chat, callback.MessageId, text, ParseMode.Html,
                    replyMarkup: UpdateKeyboard, cancellationToken: token);
            }
            catch (ApiRequestException ex) when (ex.IsMessageNotModified()) {
                // Telegram rejects edits that leave both the message and keyboard unchanged.
            }
        }
        else {
            await ctx.Bot.SendMessage(ctx.Event.Chat, text, ParseMode.Html,
                replyMarkup: UpdateKeyboard, cancellationToken: token);
        }
    }
}
