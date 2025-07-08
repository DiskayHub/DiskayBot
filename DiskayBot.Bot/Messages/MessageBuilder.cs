using System;
using System.Text;

namespace DiskayBot.Bot.Controllers;

public class MessageBuilder {
    private StringBuilder message = new();

    private MessageBuilder() {}

    public static MessageBuilder CreateMessage() {
        return new MessageBuilder();
    }

    public void AddGroupName(string group_name) {
        message.Append($"*{group_name}*\n");
    }

    public void AddText(string text) {
        message.Append(text + '\n');
    }

    public string GetMessage() {
        return message.ToString();
    }
}
