using DiskayBot.Bot.Attributes;
using DiskayBot.Bot.Bot.Messages;
using DiskayBot.Bot.DTOs;
using DiskayBot.Bot.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace DiskayBot.Bot.Bot.Commands;

[CommandName("/about")]
public class AboutCommand : IBaseCommand {
    private readonly string Version = "1.2.1-alfa";

    public async Task ExecuteAsync(BotContext ctx, CancellationToken token) {
        await ctx.Bot.SendMessage(ctx.Event.Chat, MessageBuilder.AboutBot(Version), ParseMode.Html, cancellationToken: token);
    }
}
