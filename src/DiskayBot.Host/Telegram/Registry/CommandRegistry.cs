using DiskayBot.Host.Telegram.Commands.Base;

namespace DiskayBot.Host.Telegram.Registry;

public class CommandRegistry {
    private readonly Dictionary<string, CommandDescriptor> _commands;
    private readonly Dictionary<string, CommandDescriptor> _callbacks;

    public CommandRegistry(IEnumerable<CommandDescriptor> descriptors) {
        _commands = descriptors
            .Where(d => d.Kind == HandlerKind.Command)
            .ToDictionary(x => x.Name);

        _callbacks = descriptors
            .Where(d => d.Kind == HandlerKind.Callback)
            .ToDictionary(x => x.Name);
    }

    public CommandDescriptor? FindCommand(string commandName)
        => _commands.GetValueOrDefault(commandName);

    public CommandDescriptor? FindCallback(string callbackName)
        => _callbacks.GetValueOrDefault(callbackName);
}
