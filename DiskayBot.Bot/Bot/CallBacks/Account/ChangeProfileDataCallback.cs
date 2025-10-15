using DiskayBot.Bot.Abstractions;
using DiskayBot.Bot.Bot.Controllers;
using DiskayBot.Bot.Bot.Exeptions;
using DiskayBot.Bot.Messages;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DiskayBot.Bot.Bot.CallBacks.Account;

public class ChangeProfileDataCallback : BotCommand {
    private readonly MemoryController _memoryController;
    
    public ChangeProfileDataCallback(string name, MemoryController memoryController) : base(name) {
        _memoryController = memoryController;
    }

    public override async Task ExecuteAsync(ITelegramBotClient bot, CancellationToken token, UserEvent evt) {
        var user = await _memoryController.GetUser(evt.UserId);
        if (user != null) {
            await bot.EditMessageText(
                evt.Chat,
                evt.MessageId,
                MessageBuilder.ShowProfile(user),
                ParseMode.Markdown,
                replyMarkup: GetKeyboard()
            );
        }
        else {
            throw new NotAuthorizatedExeption();
        }
    }

    public InlineKeyboardMarkup GetKeyboard() {
        var buttons = new[] {
            InlineKeyboardButton.WithCallbackData("Изменить группу", "changeCourse")
        };
        return new InlineKeyboardMarkup(buttons);
    }
}