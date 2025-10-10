using System.Data;
using DiskayBot.API.Services;
using DiskayBot.Bot.Abstractions;
using DiskayBot.Bot.Bot.Registers;
using DiskayBot.Bot.Events;
using DiskayBot.Bot.Events.Data;
using DiskayBot.Bot.Events.Internal;
using DiskayBot.Redis;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using BotCommand = DiskayBot.Bot.Abstractions.BotCommand;

namespace DiskayBot.Bot.Bot.CallBacks.Data;

public class ChooseGroupCallback : BotCommand {
    private readonly string _nextCallback;
    private readonly string _backCourse;
    private readonly UserService _userService;
    private readonly bool _saveAsName;
    public ChooseGroupCallback(string callback, UserService service, RedisController redis, EventRegister eventRegister, 
        string nextNextCallback, string backCourse, bool saveAsName = false) : base(callback) {
        _userService = service;
        _nextCallback = nextNextCallback;
        _backCourse = backCourse;
        _saveAsName = saveAsName;
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
        
        var buttons = allGroups.Select(group => {
                if (_saveAsName) {
                    return InlineKeyboardButton.WithCallbackData(group.name, $"{_nextCallback}={group.name}");   
                }
                return InlineKeyboardButton.WithCallbackData(group.name, $"{_nextCallback}={group.id}");
            }
        ).ToArray();
        
        var keyboard = new InlineKeyboardMarkup(new[] {
            buttons, 
            new[] { InlineKeyboardButton.WithCallbackData("<-- Вернуться назад", _backCourse) }
        });

        return keyboard;
    }
}