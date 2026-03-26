using DiskayBot.Host.Telegram.Attributes;
using DiskayBot.Host.Telegram.DTOs;
using DiskayBot.Host.Abstractions;

namespace DiskayBot.Host.Features.Admin.Commands;

[CommandName("/admin", AccessLevel.Admin)]
public class AdminCommand : IBaseCommand {
    public Task ExecuteAsync(BotContext ctx, CancellationToken token) {
        throw new NotImplementedException();
    }
}
