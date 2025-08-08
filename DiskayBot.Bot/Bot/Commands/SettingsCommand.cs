using DiskayBot.Bot.Abstractions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace DiskayBot.Bot.Bot.Commands;

// public class SettingsCommand : AbstractBotCommand {
//     
//     public SettingsCommand() : base("/settings") {}
//
//     public override Task ExecuteAsync(TelegramBotClient botClient, Update update, CancellationToken cancellationToken) {
//         var chat = update.Message.Chat;
//         var userId = update.Message.From.Id;
//         var username = update.Message.From.Username;
//     }
//
//     public override ReplyMarkup GetKeyboard() {
//         var buttons = new[] {
//             InlineKeyboardButton.WithCallbackData("Изменить данные о профиле", "profile_changeInfo")
//         };
//         
//         var keyboard = new InlineKeyboardMarkup(buttons);
//         return keyboard;
//     }
// }