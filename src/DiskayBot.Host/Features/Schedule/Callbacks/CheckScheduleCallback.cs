using DiskayBot.Host.Telegram.Attributes;
using DiskayBot.Host.Presentation.Messages;
using DiskayBot.Host.Telegram.DTOs;
using DiskayBot.Host.Telegram.Events;
using DiskayBot.Host.Abstractions;
using DiskayBot.Host.Features.Schedule;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DiskayBot.Host.Features.Schedule.Callbacks;

[CallbackName("checkingSchedule", AccessLevel.User)]
public class CheckScheduleCallback : IBaseCommand {
    private readonly IScheduleController _scheduleService;

    public CheckScheduleCallback(IScheduleController scheduleService) {
        _scheduleService = scheduleService;
    }

    public async Task ExecuteAsync(BotContext ctx, CancellationToken token) {
        var callbackEvent = (CallbackQueryUserEvent)ctx.Event;
        if (callbackEvent.Query != null) {
            var schedule = await _scheduleService.GetActualSchedule($"ИТ{callbackEvent.Query}");
            if (schedule != null) {
                var course = callbackEvent.QueryArgs?[0] ?? callbackEvent.Query;
                var keyboard = new InlineKeyboardMarkup(new[] {
                    new[] { InlineKeyboardButton.WithCallbackData("<-- Вернуться назад", $"checkGroup={course}") }
                });
                await ctx.Bot.EditMessageText(
                    ctx.Event.Chat,
                    ctx.Event.MessageId,
                    MessageBuilder.ShowSchedule(schedule),
                    replyMarkup: keyboard,
                    parseMode: ParseMode.Html,
                    cancellationToken: token
                );
            }
        }
    }
}
