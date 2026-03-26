using DiskayBot.Host.Telegram.Commands.Base;
using DiskayBot.Host.Telegram.Exceptions;
using DiskayBot.Host.Telegram.Registry;
using DiskayBot.Host.Telegram.DTOs;
using DiskayBot.Host.Telegram.Events;
using DiskayBot.Host.Features.Admin;
using DiskayBot.Host.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DiskayBot.Host.Telegram;

public class CommandDispatcher {
    private readonly CommandRegistry _commandRegistry;
    private readonly IServiceProvider _provider;
    private readonly AdminOptions _adminOptions;

    public CommandDispatcher(CommandRegistry registry, IServiceProvider provider, IOptions<AdminOptions> adminOptions) {
        _commandRegistry = registry;
        _provider = provider;
        _adminOptions = adminOptions.Value;
    }

    public async Task DispatchAsync(BotContext ctx, CancellationToken cancellationToken) {
        CommandDescriptor? descriptor;

        if (ctx.Event is CallbackQueryUserEvent) {
            descriptor = _commandRegistry.FindCallback(ctx.Event.GetContent());
        }
        else {
            var commandName = ctx.Event.GetContent().Split(' ')[0];
            descriptor = _commandRegistry.FindCommand(commandName);
        }

        if (descriptor == null)
            return;

        if (descriptor.AccessLevel >= AccessLevel.Admin) {
            if (ctx.Event.UserId != _adminOptions.AdminId)
                throw new NotAdminException();
        }
        else if (descriptor.AccessLevel >= AccessLevel.User && ctx.User == null) {
            throw new NotAuthorizatedExсeption();
        }

        ctx.Descriptor = descriptor;
        var command = (IBaseCommand)_provider.GetRequiredService(descriptor.CommandType);
        ctx.Command = command;

        await command.ExecuteAsync(ctx, cancellationToken);
    }
}
