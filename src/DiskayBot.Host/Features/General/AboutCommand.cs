using DiskayBot.Host.Telegram.Attributes;
using DiskayBot.Host.Presentation.Messages;
using DiskayBot.Host.Telegram.DTOs;
using DiskayBot.Host.Abstractions;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace DiskayBot.Host.Features.General;

[CommandName("/about")]
public class AboutCommand : IBaseCommand {
    private readonly string Version = "1.2.4-alfa";

    public async Task ExecuteAsync(BotContext ctx, CancellationToken token) {
        await ctx.Bot.SendMessage(ctx.Event.Chat, MessageBuilder.AboutBot(Version), ParseMode.Html, cancellationToken: token);
    }
}
