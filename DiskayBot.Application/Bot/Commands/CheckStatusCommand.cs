using DiskayBot.Bot.Attributes;
using DiskayBot.Bot.Bot.Controllers;
using DiskayBot.Bot.Bot.Messages;
using DiskayBot.Bot.DTOs;
using DiskayBot.Bot.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace DiskayBot.Bot.Bot.Commands;

[CommandName("/check_bot_status")]
public class CheckStatusCommand : IBaseCommand {
    private readonly MemoryController _memoryController;

    public CheckStatusCommand(MemoryController memoryController) {
        _memoryController = memoryController;
    }

    public async Task ExecuteAsync(BotContext ctx, CancellationToken token) {
        var userService = await _memoryController.PingService();
        await ctx.Bot.SendMessage(ctx.Event.Chat, MessageBuilder.CheckBotStatus([userService]), ParseMode.Markdown, cancellationToken: token);
    }
}
