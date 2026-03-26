using DiskayBot.Host.Telegram.Attributes;
using DiskayBot.Host.Presentation.Keyboards.Scripts;
using DiskayBot.Host.Presentation.Messages;
using DiskayBot.Host.Telegram.DTOs;
using DiskayBot.Host.Telegram.Events;
using DiskayBot.Host.Abstractions;
using DiskayBot.Host.Features.Schedule;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DiskayBot.Host.Features.Schedule.Callbacks;

[CallbackName("prevSchedule", AccessLevel.User)]
public class PrevScheduleCallback : IBaseCommand {
    private readonly IScheduleController _schedule;

    public PrevScheduleCallback(IScheduleController schedule) {
        _schedule = schedule;
    }

    public async Task ExecuteAsync(BotContext ctx, CancellationToken token) {
        var callbackEvent = (CallbackQueryUserEvent)ctx.Event;

        if (!DateOnly.TryParseExact(callbackEvent.Query, "dd.MM.yyyy", null, System.Globalization.DateTimeStyles.None, out var date)) {
            await ctx.Bot.AnswerCallbackQuery(callbackEvent.Id, cancellationToken: token);
            return;
        }

        var schedule = await _schedule.GetPreviousSchedule($"ИТ{ctx.User!.group_name}", date);
        if (schedule == null) {
            await ctx.Bot.AnswerCallbackQuery(callbackEvent.Id, "Нет предыдущего расписания", cancellationToken: token);
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