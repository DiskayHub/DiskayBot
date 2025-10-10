using DiskayBot.API.Services;
using DiskayBot.Bot.Abstractions;
using DiskayBot.Bot.Bot.Controllers;
using DiskayBot.Bot.Bot.Exeptions;
using DiskayBot.Bot.Events;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DiskayBot.Bot.Bot.Commands;

public class CheckSchedulesCommand : BotCommand {
    private readonly UserController _userController;
    private readonly string _nextCallback;
    
    public CheckSchedulesCommand(string name, UserController userController, string nextCallback) : base(name) {
        _userController = userController;
        _nextCallback = nextCallback;
    }

    public override async Task ExecuteAsync(ITelegramBotClient bot, CancellationToken token, UserEvent evt) {
        var messageText = "Узнать расписание 🔍";
        if (await _userController.IsAuthenticated(evt.UserId)) {
            try {
                var callbackEvent = (CallbackQueryUserEvent)evt;
                await bot.EditMessageText(
                    evt.Chat,
                    evt.MessageId,
                    messageText,
                    ParseMode.Markdown,
                    replyMarkup: GetKeyboard());
            }
            catch (InvalidCastException ex) {
                await bot.SendMessage(
                    evt.Chat, 
                    messageText, 
                    ParseMode.Markdown,
                    replyMarkup: GetKeyboard());      
            }
        }
        else {
            throw new NotAuthorizatedExeption();   
        }
    }

    private InlineKeyboardMarkup GetKeyboard() {
        var courses = new Dictionary<string, string> {
            {"1", ""},
            {"2", ""},
            {"3", ""},
            {"4", ""}
        };
        var keyboardRows = courses.Select(dictObject =>
            InlineKeyboardButton.WithCallbackData($"{dictObject.Key} курс {dictObject.Value}", $"{_nextCallback}={dictObject.Key}")
        ).ToList();
        
        var keyboard = new InlineKeyboardMarkup(keyboardRows);
        
        return keyboard;
    }
}