using DiskayBot.Bot.Abstractions;
using Telegram.Bot;

namespace DiskayBot.Bot.Interfaces;

public interface UserController {
    public ICommand? GetCommand(UserEvent evt);
}