using DiskayBot.Bot.Interfaces;
using Telegram.Bot;

namespace DiskayBot.Bot.Abstractions;

public abstract class BotCommand : ICommand {
    public string Name { get; }

    public BotCommand(string name) {
        Name = name;
    }

    public abstract Task ExecuteAsync(ITelegramBotClient bot, CancellationToken token, UserEvent evt);
}