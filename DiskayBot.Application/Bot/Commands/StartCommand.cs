using DiskayBot.Bot.Attributes;
using DiskayBot.Bot.DTOs;
using DiskayBot.Bot.Interfaces;
using DiskayBot.Bot.Messages;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace DiskayBot.Bot.Bot.Commands;

[CommandName("/start")]
public class StartCommand : IBaseCommand {
    public async Task ExecuteAsync(BotContext ctx, CancellationToken token) {
        await ctx.Bot.SendMessage(ctx.Event.Chat, MessageBuilder.StartMessage(), ParseMode.Markdown, cancellationToken: token);
    }
}
