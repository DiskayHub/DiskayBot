using DiskayBot.Bot.Interfaces;

namespace DiskayBot.Bot;

public class CommandRegister {
    private readonly Dictionary<string, ICommand> _commands;

    public CommandRegister(List<ICommand> commands) {
        _commands = commands.ToDictionary(c => c.Name);
    }

    public ICommand? GetCommand(string commandName) {
        return _commands.TryGetValue(commandName, out var command) ? command : null;
    }
}