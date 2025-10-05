using DiskayBot.Bot.Abstractions;
using DiskayBot.Bot.Bot.Controllers;
using DiskayBot.Bot.Bot.Exeptions;
using DiskayBot.Bot.Messages;
using DiskayBot.Redis;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using BotCommand = DiskayBot.Bot.Abstractions.BotCommand;

namespace DiskayBot.Bot.Bot.Commands;

public class ShowProfileCommand : BotCommand {
    private readonly UserController _userController;
    public ShowProfileCommand(string name, UserController userController) : base(name)  {
        _userController = userController;
    }
    public override async Task ExecuteAsync(ITelegramBotClient bot, CancellationToken token, UserEvent evt) {
        var user = await _userController.GetUserData(evt.UserId);
        if (user != null) {
            string result = MessageBuilder.ShowProfile(user);
            await bot.SendMessage(evt.Chat, result, ParseMode.Markdown);
        }
        else {
            throw new NotAuthorizatedExeption();   
        }
    }
}