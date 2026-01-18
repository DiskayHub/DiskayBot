using DiskayBot.Bot.Abstractions;
using DiskayBot.Bot.Attributes;
using DiskayBot.Bot.DTOs;
using DiskayBot.Bot.Interfaces;
using Telegram.Bot;

namespace DiskayBot.Bot.Bot.Commands;

[CommandName("/admin")]
[CommandAccess(AccessLevel.Admin)]
public class AdminCommand : IBaseCommand {
    
    public Task ExecuteAsync(ITelegramBotClient bot, CancellationToken token, UserEvent evt) {
        throw new NotImplementedException();
    }
}