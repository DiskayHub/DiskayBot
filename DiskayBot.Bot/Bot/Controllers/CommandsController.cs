using System;
using DiskayBot.API.Services;
using DiskayBot.Bot.Abstractions;
using DiskayBot.Bot.Bot.Commands;
using DiskayBot.Redis;
using Telegram.Bot;

namespace DiskayBot.Bot.Bot.Controllers;

public class CommandsController {
    private readonly Dictionary<string, AbstractBotCommand> _commands;

    public CommandsController(RedisController redis, UserService userService, ScheduleService scheduleService) {
        _commands = new Dictionary<string, AbstractBotCommand> {
            { "/start", new StartCommand() },
            { "/create_account", new RegisterCommand(redis, userService) },
            { "/show_profile", new ShowProfileCommand(redis, userService) },
            { "/check_bot_status", new CheckServicesStatus(userService, scheduleService) }
        };
    }

    public ICommand? GetCommand(string command) {
        _commands.TryGetValue(command, out var result);
        return result;
    }
}
