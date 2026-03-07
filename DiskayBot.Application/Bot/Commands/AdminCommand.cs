using DiskayBot.Bot.Attributes;
using DiskayBot.Bot.DTOs;
using DiskayBot.Bot.Interfaces;

namespace DiskayBot.Bot.Bot.Commands;

[CommandName("/admin", AccessLevel.Admin)]
public class AdminCommand : IBaseCommand {
    public Task ExecuteAsync(BotContext ctx, CancellationToken token) {
        throw new NotImplementedException();
    }
}
