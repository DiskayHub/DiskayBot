using System.Reflection;
using DiskayBot.Host.Telegram.Attributes;
using DiskayBot.Host.Abstractions;

namespace DiskayBot.Host.Telegram.Commands.Base;

public static class CommandScanner {
    public static IEnumerable<CommandDescriptor> Scan(Assembly assembly) {
        var types = assembly.GetTypes()
            .Where(t => !t.IsAbstract && typeof(IBaseCommand).IsAssignableFrom(t));

        foreach (var type in types) {
            var hasCommand = type.GetCustomAttribute<CommandNameAttribute>() != null;
            var hasCallback = type.GetCustomAttribute<CallbackNameAttribute>() != null;

            if (hasCommand)
                yield return new CommandDescriptor(type, HandlerKind.Command);

            if (hasCallback)
                yield return new CommandDescriptor(type, HandlerKind.Callback);
        }
    }
}
