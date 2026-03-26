using DiskayBot.Host.Telegram.DTOs;

namespace DiskayBot.Host.Abstractions;

public interface IBaseCommand {
    Task ExecuteAsync(BotContext ctx, CancellationToken token);
}
