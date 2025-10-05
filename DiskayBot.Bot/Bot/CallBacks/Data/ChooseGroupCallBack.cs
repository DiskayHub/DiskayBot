using System.Data;
using DiskayBot.API.Clients;
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
    private readonly UserClient _userClient;
    public ChooseGroupCallback(string callback, UserClient client, RedisController redis, EventRegister eventRegister, string nextNextCallback, string backCourse) : base(callback) {
        _userClient = client;
        _nextCallback = nextNextCallback;
        _backCourse = backCourse;
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
        var allGroups = await _userClient.GetCourseGroups(course);
        
        allGroups = allGroups.OrderBy(c => {
            var parts = c.name.Split('-');
            return int.Parse(parts[1]);
        }).ToList();
        
        var buttons = allGroups.Select(group => 
            InlineKeyboardButton.WithCallbackData(group.name, $"{_nextCallback}={group.id}")
        ).ToArray();
        
        var keyboard = new InlineKeyboardMarkup(new[] {
            buttons, 
            new[] { InlineKeyboardButton.WithCallbackData("<-- Вернуться назад", _backCourse) }
        });

        return keyboard;
    }
}