using DiskayBot.API.Services;
using DiskayBot.Bot.Abstractions;
using StackExchange.Redis;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DiskayBot.Bot.Bot.Commands;

public class RegisterCommand : AbstractBotCommand {
    public RegisterCommand() : base("/create_account") {}

    public override async Task ExecuteAsync(TelegramBotClient botClient, Update update, CancellationToken cancellationToken) {
        var keyboard = await GetReplyMarkup();
        await botClient.SendMessage(
            update.Message!.Chat,
            "Выберите группу",
            ParseMode.Markdown,
            replyMarkup: keyboard
        );
    }

    public async Task<ReplyMarkup?> GetReplyMarkup() {
        var groups = await BotService.GetAllGroups();

        var keyboard_rows = groups.Select(item =>
            new[] { InlineKeyboardButton.WithCallbackData(item.name, $"group_{item.id}") }
        ).ToList();
        var keyboard = new InlineKeyboardMarkup(keyboard_rows);
        return keyboard;
    }
}
