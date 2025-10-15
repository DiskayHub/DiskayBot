using DiskayBot.Bot.Abstractions;
using DiskayBot.Bot.Events;
using DiskayBot.Bot.Messages;
using DiskayBot.Redis;
using DiskayBot.Services.ScheduleService.Components;
using DiskayBot.Services.ScheduleService.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DiskayBot.Bot.Bot.CallBacks.Schedule;

public class CheckScheduleCallback : BotCommand {
    private readonly IScheduleController _scheduleService;
    private readonly RedisController _redisController;
    private readonly string _previosCallback;
    
    public CheckScheduleCallback(string name, IScheduleController scheduleService, RedisController redisController, string previosCallback) : base(name) {
        _scheduleService = scheduleService;
        _redisController = redisController;
        _previosCallback = previosCallback;
    }

    public override async Task ExecuteAsync(ITelegramBotClient bot, CancellationToken token, UserEvent evt) {
        var callbackEvent = (CallbackQueryUserEvent)evt;
        if (callbackEvent.Query != null) {
            var schedule = _scheduleService.GetActualSchedule($"ИТ{callbackEvent.Query}");
            if (schedule != null) {
                await bot.EditMessageText(
                    callbackEvent.Chat, 
                    callbackEvent.MessageId,
                    MessageBuilder.ShowSchedule(schedule, false),
                    replyMarkup: GetKeyboard(callbackEvent.QueryArgs[0] ?? callbackEvent.Query),
                    parseMode: ParseMode.Html
                );   
            }
        }
        else {
            throw new Exception();   
        }
    }

    private InlineKeyboardMarkup GetKeyboard(string course) {
        return new InlineKeyboardMarkup(new[] {
            new[] { InlineKeyboardButton.WithCallbackData("<-- Вернуться назад", $"{_previosCallback}={course}") }
        });
    }
}