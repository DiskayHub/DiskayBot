using System;
using DiskayBot.Bot.Abstractions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DiskayBot.Bot;

public class StartCommand : AbstractBotCommand {
    public StartCommand() : base("/start") {}

    public override async Task ExecuteAsync(TelegramBotClient botClient, Update update, CancellationToken cts_token) {
        var chat_id = update.Message!.Chat;
        await botClient.SendMessage(chat_id, "Привет, я ✨✨ 𝔻𝕚𝕤𝕜𝕒𝕪 ✨✨", ParseMode.Markdown);
    }
}
