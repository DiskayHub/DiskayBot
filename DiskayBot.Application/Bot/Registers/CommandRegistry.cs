using DiskayBot.Bot.Abstractions;
using DiskayBot.Bot.Bot.Commands.Base;

namespace DiskayBot.Bot.Bot.Registers;

public class CommandRegistry {
    private Dictionary<string, CommandDescriptor> _commands;
    
    public CommandRegistry(IEnumerable<CommandDescriptor> commands) {
        _commands = commands.ToDictionary(x => x.Name);
    }
    
    public CommandDescriptor? Find(string commandName)
        => _commands.GetValueOrDefault(commandName);
}