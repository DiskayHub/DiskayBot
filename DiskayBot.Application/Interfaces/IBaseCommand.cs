using DiskayBot.Bot.DTOs;

namespace DiskayBot.Bot.Interfaces;

public interface IBaseCommand {
    Task ExecuteAsync(BotContext ctx, CancellationToken token);
}
