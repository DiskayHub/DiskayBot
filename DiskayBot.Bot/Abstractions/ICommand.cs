using System;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace DiskayBot.Bot.Abstractions;

public interface ICommand {
    string Name {get;}
    public abstract Task ExecuteAsync(TelegramBotClient botClient, Update update, CancellationToken cancellationToken);
    public virtual ReplyMarkup GetKeyboard() { return null; }
}
