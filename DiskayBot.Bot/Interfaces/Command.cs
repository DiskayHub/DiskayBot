using DiskayBot.Bot.Abstractions;
using DiskayBot.Bot.Events;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace DiskayBot.Bot.Interfaces;

public interface ICommand {
    public string Name { get; }
    public abstract Task ExecuteAsync(ITelegramBotClient bot, CancellationToken token, UserEvent evt);
    public virtual ReplyMarkup GetKeyboard() { return null; }
}
