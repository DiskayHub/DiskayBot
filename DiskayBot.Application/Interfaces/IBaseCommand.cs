using DiskayBot.Bot.Abstractions;
using Telegram.Bot;

namespace DiskayBot.Bot.Interfaces;

public interface IBaseCommand {
    public Task ExecuteAsync(ITelegramBotClient bot, CancellationToken token, UserEvent evt);
}