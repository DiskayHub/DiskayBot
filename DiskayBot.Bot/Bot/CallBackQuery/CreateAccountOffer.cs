using System;
using DiskayBot.Bot.Abstractions;
using DiskayBot.Bot.Messages;
using DiskayBot.Redis;
using StackExchange.Redis;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DiskayBot.Bot.Bot.Controllers;

public class CreateAccountOffer : AbstractBotCallBack {
    private readonly RedisController _redis;

    public CreateAccountOffer(RedisController redis) : base("createAccountOffer") {
        _redis = redis;
    }

    public override async Task ExecuteAsync(TelegramBotClient botClient, Update update, CancellationToken cancellationToken, string callBack) {
        Chat chat_id = update.CallbackQuery!.Message!.Chat;
        try{  
            
            var keyboard = GetKeyboard();
            await botClient.SendMessage(chat_id,
                MessageBuilder.RegisterOffer(),
                ParseMode.Markdown,
                replyMarkup: keyboard
            );
        }
        catch (Exception e){
            throw new Exception(e.Message);
        }
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
