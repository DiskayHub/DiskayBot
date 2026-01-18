using System.Reflection;
using DiskayBot.Bot.Attributes;
using DiskayBot.Bot.Interfaces;

namespace DiskayBot.Bot.Bot.Commands.Base;

/// <summary>
/// Достаёт все комманды с аттрибутом CommandName
/// </summary>
public static class CommandScanner {
    public static IEnumerable<CommandDescriptor> Scan(Assembly assembly) {
        return assembly.GetTypes()
            .Where(t =>
                !t.IsAbstract &&
                typeof(IBaseCommand).IsAssignableFrom(t) &&
                t.GetCustomAttribute<CommandNameAttribute>() != null)
            .Select(t => new CommandDescriptor(t));
    }
}
