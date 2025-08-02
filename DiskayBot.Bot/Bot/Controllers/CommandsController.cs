using System;
using DiskayBot.API.Services;
using DiskayBot.Bot.Abstractions;
using DiskayBot.Bot.Bot.Commands;
using DiskayBot.Redis;
using Telegram.Bot;

namespace DiskayBot.Bot.Bot.Controllers;

public class CommandsController {
    private readonly Dictionary<string, AbstractBotCommand> _commands;

    public CommandsController(RedisController redis, MemoryService service) {
        _commands = new Dictionary<string, AbstractBotCommand> {
            { "/start", new StartCommand() },
            { "/create_account", new RegisterCommand(redis, service) },
            { "/show_profile", new ShowProfileCommand(redis, service) }
        };
    }

    public ICommand? GetCommand(string command) {
        _commands.TryGetValue(command, out var result);
        return result;
    }
}
