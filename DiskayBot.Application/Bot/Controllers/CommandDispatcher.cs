using DiskayBot.Bot.Bot.Commands.Base;
using DiskayBot.Bot.Bot.Exeptions;
using DiskayBot.Bot.Bot.Registers;
using DiskayBot.Bot.DTOs;
using DiskayBot.Bot.Events;
using DiskayBot.Bot.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace DiskayBot.Bot.Bot.Controllers;

public class CommandDispatcher {
    private readonly CommandRegistry _commandRegistry;
    private readonly IServiceProvider _provider;

    public CommandDispatcher(CommandRegistry registry, IServiceProvider provider) {
        _commandRegistry = registry;
        _provider = provider;
    }

    public async Task DispatchAsync(BotContext ctx, CancellationToken cancellationToken) {
        CommandDescriptor? descriptor;

        if (ctx.Event is CallbackQueryUserEvent) {
            descriptor = _commandRegistry.FindCallback(ctx.Event.GetContent());
        }
        else {
            descriptor = _commandRegistry.FindCommand(ctx.Event.GetContent());
        }

        if (descriptor == null)
            return;

        if (descriptor.AccessLevel >= AccessLevel.User && ctx.User == null)
            throw new NotAuthorizatedExeption();

        ctx.Descriptor = descriptor;
        var command = (IBaseCommand)_provider.GetRequiredService(descriptor.CommandType);
        ctx.Command = command;

        await command.ExecuteAsync(ctx, cancellationToken);
    }
}
