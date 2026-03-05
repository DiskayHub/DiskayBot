using DiskayBot.Bot.Attributes;
using DiskayBot.Bot.Bot.KeyBoard.Scripts;
using DiskayBot.Bot.DTOs;
using DiskayBot.Bot.Events;
using DiskayBot.Bot.Interfaces;
using DiskayBot.Bot.Messages;
using DiskayBot.Bot.ScheduleService;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DiskayBot.Bot.Bot.Callbacks.Schedule;

[CallbackName("nextSchedule", AccessLevel.User)]
public class NextScheduleCallback : IBaseCommand {
    private readonly IScheduleController _schedule;

    public NextScheduleCallback(IScheduleController schedule) {
        _schedule = schedule;
    }

    public async Task ExecuteAsync(BotContext ctx, CancellationToken token) {
        var callbackEvent = (CallbackQueryUserEvent)ctx.Event;

        if (!DateOnly.TryParseExact(callbackEvent.Query, "dd.MM.yyyy", null, System.Globalization.DateTimeStyles.None, out var date)) {
            await ctx.Bot.AnswerCallbackQuery(callbackEvent.Id, cancellationToken: token);
            return;
        }
        var schedule = await _schedule.GetNextSchedule($"ИТ{ctx.User!.group_name}", date);
        if (schedule == null) {
            await ctx.Bot.AnswerCallbackQuery(callbackEvent.Id, "Нет следующего расписания", cancellationToken: token);
            return;
        }

        try {
            await ctx.Bot.EditMessageText(
                ctx.Event.Chat,
                ctx.Event.MessageId,
                MessageBuilder.ShowSchedule(schedule),
                replyMarkup: GlobalKeyboard.GetScheduleNavigatorKeyboard(schedule.date),
                parseMode: ParseMode.Html,
                cancellationToken: token
            );
        }
        catch (ApiRequestException) {
            await ctx.Bot.AnswerCallbackQuery(callbackEvent.Id, cancellationToken: token);
        }
    }
}