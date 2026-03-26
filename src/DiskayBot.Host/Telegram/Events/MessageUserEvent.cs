using DiskayBot.Host.Telegram.Abstractions;
using Telegram.Bot;

namespace DiskayBot.Host.Telegram.Events;

public class MessageUserEvent : UserEvent {
    private readonly string _textMessage;
    
    public MessageUserEvent(UpdateInfo info, string textMessage) : base(info) {
        _textMessage = textMessage;
    }

    public override string GetContent() {
        return _textMessage;
    }
}