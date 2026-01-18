using System.Reflection;
using DiskayBot.Bot.Attributes;
using DiskayBot.Bot.DTOs;

namespace DiskayBot.Bot.Bot.Commands.Base;

public class CommandDescriptor {
    public Type CommandType { get; }
    public string Name { get; }
    public AccessLevel AccessLevel { get; }

    public CommandDescriptor(Type type) {
        CommandType = type;

        var nameAttr = type.GetCustomAttribute<CommandNameAttribute>() ?? throw new ArgumentException($"{nameof(type)} must have a {nameof(CommandNameAttribute)}");
        Name = nameAttr.Name;

        var accessAttr = type.GetCustomAttribute<CommandAccessAttribute>();
        AccessLevel = accessAttr?.AccessLevel ?? AccessLevel.None;
    }
}