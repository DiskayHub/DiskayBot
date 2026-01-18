using DiskayBot.API.Contracts;
using DiskayBot.Bot.Abstractions;
using Telegram.Bot;

namespace DiskayBot.Bot.Interfaces;

public interface IAuthCommand {
    public Task ExecuteAsync(ITelegramBotClient bot, CancellationToken token, UserData user);
}