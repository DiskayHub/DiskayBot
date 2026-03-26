using DiskayBot.Infrastructure.Contracts;
using DiskayBot.Host.Telegram.Abstractions;
using DiskayBot.Host.Telegram.Commands.Base;
using DiskayBot.Host.Abstractions;
using Telegram.Bot;

namespace DiskayBot.Host.Telegram.DTOs;

public sealed class BotContext {
    public ITelegramBotClient Bot { get; init; }
    public UserEvent Event { get; init; }
    public IBaseCommand? Command { get; set; }
    public UserData? User { get; set; }
    public CommandDescriptor? Descriptor { get; set; }
}
