using DiskayBot.API.Clients;
using DiskayBot.API.Services;
using DiskayBot.Bot.Abstractions;
using DiskayBot.Bot.Messages;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace DiskayBot.Bot.Bot.Commands;

public class CheckStatusCommand : BotCommand {
    private readonly UserClient _userClient;
    
    public CheckStatusCommand(string name, UserClient userClient) : base(name) {
        _userClient = userClient;
    }

    public override async Task ExecuteAsync(ITelegramBotClient bot, CancellationToken token, UserEvent evt) {
        var userService = await _userClient.PingService();
        await bot.SendMessage(evt.Chat, MessageBuilder.CheckBotStatus([userService]), ParseMode.Markdown);
    }
}