using DiskayBot.Bot.DTOs;

namespace DiskayBot.Bot.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class CallbackNameAttribute : Attribute {
    public string Name { get; }
    public AccessLevel AccessLevel { get; }

    public CallbackNameAttribute(string name, AccessLevel accessLevel = AccessLevel.None) {
        Name = name;
        AccessLevel = accessLevel;
    }
}
