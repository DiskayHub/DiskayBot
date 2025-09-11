using System.Data;
using DiskayBot.API.Services;
using DiskayBot.Bot.Abstractions;
using DiskayBot.Bot.Bot.Events;
using DiskayBot.Bot.Bot.Registers;
using DiskayBot.Bot.Events;
using DiskayBot.Bot.Events.Data;
using DiskayBot.Redis;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using BotCommand = DiskayBot.Bot.Abstractions.BotCommand;

namespace DiskayBot.Bot.Bot.CallBacks.Data;

public class ChooseGroupCallback : BotCommand {
    private readonly string _nextCallback;
    private readonly UserService _userService;
    public ChooseGroupCallback(string callback, UserService service, RedisController redis, EventRegister eventRegister, string nextNextCallback) : base(callback) {
        _userService = service;
        _nextCallback = nextNextCallback;
        eventRegister.HandleEvent(nextNextCallback, new SaveGroupHandler("Сохранение группы", redis));
    }
    
    public override async Task ExecuteAsync(ITelegramBotClient bot, CancellationToken token, UserEvent evt) {
        var callBackEvent = (CallbackQueryUserEvent)evt;

        if (callBackEvent.Query != null) {
            var course = short.Parse(callBackEvent.Query);
            var keyboard = await GetInlineKeyboard(course);
            await bot.EditMessageText(
                chatId: callBackEvent.Chat,
                messageId: callBackEvent.MessageId,
                text: $"*Курс: {course}*\n\nВыберите группу:",
                parseMode: ParseMode.Markdown,
                replyMarkup: keyboard
            );
        }
    }

    public async Task<InlineKeyboardMarkup> GetInlineKeyboard(short course) {
        var allGroups = await _userService.GetCourseGroups(course);
        
        allGroups = allGroups.OrderBy(c => {
            var parts = c.name.Split('-');
            return int.Parse(parts[1]);
        }).ToList();
        
        var buttons = allGroups.Select(group => 
            InlineKeyboardButton.WithCallbackData(group.name, $"{_nextCallback}={group.id}")
        ).ToArray();
        
        var keyboard = new InlineKeyboardMarkup(new[] {
            buttons, 
            new[] { InlineKeyboardButton.WithCallbackData("<-- Вернуться назад", "chooseCourse") }
        });

        return keyboard;
    }
}