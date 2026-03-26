using DiskayBot.Host.Telegram.Abstractions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DiskayBot.Host.Telegram.Events.Base;

public static class EventCreator {
    public static UserEvent Create(Update update) {
        var updateInfo = UpdateParser.Parse(update);

        if (updateInfo != null){
            switch (updateInfo.Type){
                case UpdateType.Message:
                    return new MessageUserEvent(updateInfo, update.Message.Text);
                case UpdateType.CallbackQuery:
                    return new CallbackQueryUserEvent(updateInfo, update.CallbackQuery.Data,  update.CallbackQuery.Id);
            } 
        }
        throw new Exception("Unknown update type");
    }
}