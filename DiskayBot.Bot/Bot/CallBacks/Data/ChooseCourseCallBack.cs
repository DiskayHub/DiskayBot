using DiskayBot.Bot.Abstractions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using BotCommand = DiskayBot.Bot.Abstractions.BotCommand;

namespace DiskayBot.Bot.Bot.CallBacks.Data;

public class ChooseCourseCallBack : BotCommand {
    private readonly string _nextCallback;
    
    public ChooseCourseCallBack(string callback, string nextCallback) : base(callback) {
        _nextCallback = nextCallback;
    }
    public override async Task ExecuteAsync(ITelegramBotClient bot, CancellationToken token, UserEvent evt) {
        var keyboard = GetReplyMarkup();

        await bot.EditMessageText(
            evt.Chat,
            evt.MessageId,
            "Выберите курс",
            ParseMode.Markdown,
            replyMarkup: keyboard
        );
    }
    
    public InlineKeyboardMarkup GetReplyMarkup() {
        var courses = new Dictionary<string, string> {
            {"1", ""},
            {"2", ""},
            {"3", ""},
            {"4", ""}
        };

        var keyboardRows = courses.Select(dictObject =>
            new[] { InlineKeyboardButton.WithCallbackData($"{dictObject.Key} курс {dictObject.Value}", $"{_nextCallback}={dictObject.Key}") }
        ).ToList();
        
        
        var keyboard = new InlineKeyboardMarkup(keyboardRows);
        
        return keyboard;
    }
}