using DiskayBot.Bot.Abstractions;
using DiskayBot.Bot.Bot.Controllers;
using DiskayBot.Bot.Bot.Exeptions;
using DiskayBot.Bot.Bot.KeyBoard;
using DiskayBot.Bot.Interfaces;
using DiskayBot.Bot.Messages;
using DiskayBot.Redis;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using BotCommand = DiskayBot.Bot.Abstractions.BotCommand;

namespace DiskayBot.Bot.Bot.Commands;

public class RegisterCommand : BotCommand {
    private readonly InlineKeyboardMarkup _keyboard;
    private readonly MemoryController _memoryController;

    public RegisterCommand(string name, MemoryController memoryController, string callback) : base(name) {
        _memoryController = memoryController;

        _keyboard = new InlineKeyboardMarkup(new InlineKeyboardButton[] {
            InlineKeyboardButton.WithCallbackData("Продолжить", callback) 
        });
    }

    public override async Task ExecuteAsync(ITelegramBotClient botClient, CancellationToken token, UserEvent evt) {
        if (!await _memoryController.UserIsAuthenticated(evt.UserId)) {
            await botClient.SendMessage(
                evt.Chat,
                MessageBuilder.CreateAccount(),
                ParseMode.Markdown,
                replyMarkup: _keyboard
            );
        }
        else{
            await botClient.SendMessage(evt.Chat, "Кажется, вы уже авторизованы.", ParseMode.Markdown);
        }
    }
}