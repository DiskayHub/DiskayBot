using DiskayBot.Bot.DTOs;

namespace DiskayBot.Bot.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class CommandNameAttribute : Attribute {
    public string Name { get; set; }
    public AccessLevel AccessLevel { get; set; }
    public CommandNameAttribute(string name, AccessLevel accessLevel = AccessLevel.None) {
        Name = name;
        AccessLevel = accessLevel;
    }
}