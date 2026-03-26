using DiskayBot.Host.Telegram.Attributes;
using DiskayBot.Host.Features.Account;
using DiskayBot.Host.Presentation.Messages;
using DiskayBot.Host.Telegram.DTOs;
using DiskayBot.Host.Abstractions;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace DiskayBot.Host.Features.General;

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
