using System;
using DiskayBot.Bot.Abstractions;
using DiskayBot.Redis;
using StackExchange.Redis;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DiskayBot.Bot.Bot.Controllers;

public class ChoseGroupCallback : AbstractBotCallBack {
    private readonly RedisController _redis;

    public ChoseGroupCallback(RedisController redis) : base("group") {
        _redis = redis;
    }

    public override async Task ExecuteAsync(TelegramBotClient botClient, Update update, CancellationToken cancellationToken, string callBack) {
        Chat chat_id = update.CallbackQuery!.Message!.Chat;
        var keyboard = GetKeyboard();
        await botClient.SendMessage(chat_id, 
            "Вы уверены? \nПосле согласия вы попадёте в память Diskay 💫", 
            ParseMode.Markdown,
            replyMarkup: keyboard
        );
        var hash = new HashEntry[] {
            new HashEntry("group_id", callBack),
        };
        await _redis.SaveDataHash(chat_id.Id.ToString(), hash, TimeSpan.FromSeconds(100));
    }
 
    public override ReplyMarkup GetKeyboard() {
        var keyboardButtons = new[] {
            new[] { InlineKeyboardButton.WithCallbackData("Да", "createAccount_yes") },
            new[] { InlineKeyboardButton.WithCallbackData("Нет", "createAccount_no") }
        };
        var keyboard = new InlineKeyboardMarkup(keyboardButtons);
        return keyboard;
    }
}
