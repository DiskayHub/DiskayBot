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
        var courses = new List<string>();
        courses = ["1", "2", "3", "4"];

        var keyboard_rows = courses.Select(course =>
            new[] { InlineKeyboardButton.WithCallbackData(course, $"{_nextCallback}={course}") }
        ).ToList();
        
        var keyboard = new InlineKeyboardMarkup(keyboard_rows);
        
        return keyboard;
    }
}