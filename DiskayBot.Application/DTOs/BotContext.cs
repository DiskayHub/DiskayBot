using DiskayBot.API.Contracts;
using DiskayBot.Bot.Abstractions;
using DiskayBot.Bot.Bot.Commands.Base;
using DiskayBot.Bot.Interfaces;
using Telegram.Bot;

namespace DiskayBot.Bot.DTOs;

public sealed class BotContext {
    public ITelegramBotClient Bot { get; init; }
    public UserEvent Event { get; init; }
    public IBaseCommand? Command { get; set; }
    public UserData? User { get; set; }
    public CommandDescriptor? Descriptor { get; set; }
}
