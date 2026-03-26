using DiskayBot.Host.Telegram.Attributes;
using DiskayBot.Host.Telegram.DTOs;
using DiskayBot.Host.Abstractions;

namespace DiskayBot.Host.Features.General;

[CommandName(("/report"), AccessLevel.User)]
public class SendReportCommand : IBaseCommand {
    public Task ExecuteAsync(BotContext ctx, CancellationToken token) {
        return Task.CompletedTask;
    }
}
