using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DiskayBot.Host.Telegram.Abstractions;

public static class UpdateParser {

    public static UpdateInfo? Parse(Update update) {
        switch (update.Type){
            case UpdateType.CallbackQuery:
                return new UpdateInfo(
                    Type: update.Type,
                    Username: update.CallbackQuery.From.Username,
                    Chat: update.CallbackQuery.Message.Chat,
                    UserId: update.CallbackQuery.From.Id,
                    MessageId:  update.CallbackQuery.Message
                );
            
            case UpdateType.Message:
                return new UpdateInfo(
                    Type: update.Type,
                    Username: update.Message.From.Username,
                    Chat: update.Message.Chat,
                    UserId: update.Message.From.Id,
                    MessageId:  update.Message
                );
        }

        return null;
    }
}