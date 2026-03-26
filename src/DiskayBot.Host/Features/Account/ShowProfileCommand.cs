using DiskayBot.Host.Telegram.Attributes;
using DiskayBot.Host.Presentation.Messages;
using DiskayBot.Host.Telegram.DTOs;
using DiskayBot.Host.Abstractions;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace DiskayBot.Host.Features.Account;

[CommandName("/show_profile", AccessLevel.User)]
public class ShowProfileCommand : IBaseCommand {
    public async Task ExecuteAsync(BotContext ctx, CancellationToken token) {
        var result = MessageBuilder.ShowProfile(ctx.User!);
        await ctx.Bot.SendMessage(ctx.Event.Chat, result, ParseMode.Markdown, cancellationToken: token);
    }
}
