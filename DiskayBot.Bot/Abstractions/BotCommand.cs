using System;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace DiskayBot.Bot.Abstractions;

public abstract class AbstractBotCommand : ICommand {
    public string Name {get;}

    public AbstractBotCommand(string command) {
        Name = command;
    }

    public abstract Task ExecuteAsync(TelegramBotClient botClient, Update update, CancellationToken cancellationToken);
    
    public virtual ReplyMarkup GetKeyboard() => null;
}
