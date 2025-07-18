using System;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace DiskayBot.Bot.Abstractions;

public interface ICallBack {
    string Name {get;}
    public abstract Task ExecuteAsync(TelegramBotClient botClient, Update update, CancellationToken cancellationToken, string? query);
    public virtual ReplyMarkup GetKeyboard() { return null; }
}
