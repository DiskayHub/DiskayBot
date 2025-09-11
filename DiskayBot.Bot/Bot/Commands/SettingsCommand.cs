using DiskayBot.Bot.Abstractions;
using DiskayBot.Bot.Bot.Controllers;
using DiskayBot.Bot.Bot.Exeptions;
using DiskayBot.Bot.Messages;
using DiskayBot.Redis;
using StackExchange.Redis;
using Telegram.Bot;

namespace DiskayBot.Bot.Bot.Commands;

public class SettingsCommand : BotCommand {
    private readonly UserController _userController;
    
    public SettingsCommand(string name, UserController userController) : base(name) {
        _userController = userController;
    }

    public override async Task ExecuteAsync(ITelegramBotClient bot, CancellationToken token, UserEvent evt) {
        if (await _userController.IsAuthenticated(evt.UserId)) {
            
        }
        else {
            throw new NotAuthorizatedExeption();   
        }
    }
}