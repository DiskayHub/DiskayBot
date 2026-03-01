using DiskayBot.Bot.Attributes;
using DiskayBot.Bot.DTOs;
using DiskayBot.Bot.Interfaces;
using DiskayBot.Bot.Messages;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace DiskayBot.Bot.Bot.Commands;

[CommandName("/show_profile", AccessLevel.User)]
public class ShowProfileCommand : IBaseCommand {
    public async Task ExecuteAsync(BotContext ctx, CancellationToken token) {
        var result = MessageBuilder.ShowProfile(ctx.User!);
        await ctx.Bot.SendMessage(ctx.Event.Chat, result, ParseMode.Markdown, cancellationToken: token);
    }
}
