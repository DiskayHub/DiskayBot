using DiskayBot.Bot.DTOs;

namespace DiskayBot.Bot.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class CommandAccessAttribute : Attribute {
    public AccessLevel AccessLevel { get; set; }

    public CommandAccessAttribute(AccessLevel accessLevel) {
        AccessLevel = accessLevel;
    }
}