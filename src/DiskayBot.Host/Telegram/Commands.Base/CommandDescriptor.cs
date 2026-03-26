using System.Reflection;
using DiskayBot.Host.Telegram.Attributes;
using DiskayBot.Host.Telegram.DTOs;

namespace DiskayBot.Host.Telegram.Commands.Base;

public enum HandlerKind {
    Command,
    Callback
}

public class CommandDescriptor {
    public Type CommandType { get; }
    public string Name { get; }
    public AccessLevel AccessLevel { get; }
    public HandlerKind Kind { get; }

    public CommandDescriptor(Type type, HandlerKind kind) {
        CommandType = type;
        Kind = kind;

        if (kind == HandlerKind.Command) {
            var nameAttr = type.GetCustomAttribute<CommandNameAttribute>()
                ?? throw new ArgumentException($"{type.Name} must have a {nameof(CommandNameAttribute)}");
            Name = nameAttr.Name;
            AccessLevel = nameAttr.AccessLevel;
        }
        else {
            var callbackAttr = type.GetCustomAttribute<CallbackNameAttribute>()
                ?? throw new ArgumentException($"{type.Name} must have a {nameof(CallbackNameAttribute)}");
            Name = callbackAttr.Name;
            AccessLevel = callbackAttr.AccessLevel;
        }
    }
}
