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

[CallbackName("updateSchedule", AccessLevel.User)]
public class UpdateScheduleCallback : IBaseCommand {
    private readonly IScheduleController _scheduleService;

    public UpdateScheduleCallback(IScheduleController scheduleService) {
        _scheduleService = scheduleService;
    }

    public async Task ExecuteAsync(BotContext ctx, CancellationToken token) {
        var callbackEvent = (CallbackQueryUserEvent)ctx.Event;
        var schedule = await _scheduleService.GetActualSchedule($"ИТ{ctx.User!.group_name}");
        if (schedule != null) {
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
}
