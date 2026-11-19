using DiskayBot.Bot.Attributes;
using DiskayBot.Bot.Bot.Exeptions;
using DiskayBot.Bot.Bot.KeyBoard.Scripts;
using DiskayBot.Bot.Bot.Messages;
using DiskayBot.Bot.DTOs;
using DiskayBot.Bot.Events;
using DiskayBot.Bot.Interfaces;
using DiskayBot.Bot.ScheduleService;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DiskayBot.Bot.Bot.Callbacks.Schedule;

[CallbackName("updateSchedule", AccessLevel.User)]
public class UpdateScheduleCallback : IBaseCommand {
    private readonly IScheduleController _scheduleService;
    private readonly ILogger<UpdateScheduleCallback> _logger;

    public UpdateScheduleCallback(IScheduleController scheduleService, ILogger<UpdateScheduleCallback> logger) {
        _scheduleService = scheduleService;
        _logger = logger;
    }

    public async Task ExecuteAsync(BotContext ctx, CancellationToken token) {
        var callbackEvent = (CallbackQueryUserEvent)ctx.Event;
        var schedule = await _scheduleService.GetActualSchedule(ctx.User!.group_name);
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
            catch (ApiRequestException ex) {
                if (!ex.IsMessageNotModified()) {
                    _logger.LogWarning(ex, "Не удалось показать расписание пользователю {UserId}", ctx.Event.UserId);
                }
                await ctx.Bot.AnswerCallbackQuery(callbackEvent.Id, cancellationToken: token);
            }
        }
    }
}
