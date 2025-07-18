using System;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DiskayBot.Bot.Abstractions;

public abstract class AbstractBotCallBack : ICallBack {
    public string Name {get;}

    public AbstractBotCallBack(string callback) {
        Name = callback;
    }

    public abstract Task ExecuteAsync(TelegramBotClient botClient, Update update, CancellationToken cancellationToken, string? query);
}
