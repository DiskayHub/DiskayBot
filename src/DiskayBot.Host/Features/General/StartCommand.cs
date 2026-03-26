using DiskayBot.Host.Telegram.Attributes;
using DiskayBot.Host.Presentation.Messages;
using DiskayBot.Host.Telegram.DTOs;
using DiskayBot.Host.Abstractions;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace DiskayBot.Host.Features.General;

[CommandName("/start")]
public class StartCommand : IBaseCommand {
    public async Task ExecuteAsync(BotContext ctx, CancellationToken token) {
        await ctx.Bot.SendMessage(ctx.Event.Chat, MessageBuilder.StartMessage(), ParseMode.Html, cancellationToken: token);
    }
}
