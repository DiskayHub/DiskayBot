using DiskayBot.Bot.Abstractions;
using DiskayBot.Bot.Bot.Registers;
using DiskayBot.Bot.DTOs;
using DiskayBot.Bot.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;

namespace DiskayBot.Bot.Bot.Controllers;

public class CommandDispatcher {
    private readonly CommandRegistry _commandRegistry;
    private readonly IServiceProvider _provider;
    
    public CommandDispatcher(CommandRegistry registry, IServiceProvider provider) {
        _commandRegistry = registry;
        _provider = provider;
    }

    public async Task DispatchAsync(BotContext ctx, CancellationToken cancellationToken) {
        var descriptor = _commandRegistry.Find(ctx.Event.GetContent());
        if (descriptor == null) {
            return;
        }
        
        var command = (IBaseCommand)_provider.GetRequiredService(descriptor.CommandType);
        ctx.Command = command;

        await command.ExecuteAsync(ctx.Bot, cancellationToken, ctx.Event);
    }
}