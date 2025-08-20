using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DiskayBot.Bot.Abstractions;

public abstract class UserEvent {
    public readonly UpdateType Type;
    public readonly Chat Chat;
    public readonly long UserId;
    public readonly string Username;
    public readonly MessageId MessageId;

    public UserEvent(UpdateInfo info) {
        Type = info.Type;
        Chat = info.Chat;
        UserId = info.UserId;
        Username = info.Username;
        MessageId = info.MessageId;
    }
    public abstract string GetContent();

    public override string ToString() {
        return $"Type: {Type}, Chat: {Chat}, UserId: {UserId}, MessageId: {MessageId}";
    }
}