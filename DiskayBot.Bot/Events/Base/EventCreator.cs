using DiskayBot.Bot.Abstractions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DiskayBot.Bot.Events.Base;

public class EventCreator {
    private readonly UpdateParser _updateParser = new();
    public EventCreator() {}

    public UserEvent? Create(Update update) {
        var updateInfo = _updateParser.Parse(update);

        if (updateInfo != null){
            switch (updateInfo.Type){
                case UpdateType.Message:
                    return new MessageUserEvent(updateInfo, update.Message.Text);
                case UpdateType.CallbackQuery:
                    return new CallbackQueryUserEvent(updateInfo, update.CallbackQuery.Data);
            } 
        }
        throw new Exception("Unknown update type");
    }
}