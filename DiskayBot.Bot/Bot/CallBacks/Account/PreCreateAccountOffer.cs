using DiskayBot.Bot.Abstractions;
using DiskayBot.Bot.Messages;
using DiskayBot.Redis;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using BotCommand = DiskayBot.Bot.Abstractions.BotCommand;

namespace DiskayBot.Bot.Bot.CallBacks.Account;

public class PreCreateAccountOffer : BotCommand {
    private readonly string _nextCallback;
    private readonly RedisController _redis;

    public PreCreateAccountOffer(string callback, RedisController redis, string nextCallback) : base(callback) {
        _redis = redis;
        _nextCallback = nextCallback;
    }

    public override async Task ExecuteAsync(ITelegramBotClient bot, CancellationToken token, UserEvent evt) {
        var keyboard = GetKeyboard();
        await bot.EditMessageText(
            evt.Chat,
            evt.MessageId,
            MessageBuilder.RegisterOffer(),
            ParseMode.Markdown,
            replyMarkup: keyboard
        );
    }

    public InlineKeyboardMarkup GetKeyboard() {
        var keyboardButtons = new[] {
            new[] { InlineKeyboardButton.WithCallbackData("Да", $"{_nextCallback}=yes") },
            new[] { InlineKeyboardButton.WithCallbackData("Нет", $"{_nextCallback}=no") }
        };
        var keyboard = new InlineKeyboardMarkup(keyboardButtons);
        return keyboard;
    }
}