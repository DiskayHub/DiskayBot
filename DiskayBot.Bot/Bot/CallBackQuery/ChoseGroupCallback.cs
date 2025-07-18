using System;
using DiskayBot.Bot.Abstractions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DiskayBot.Bot.Bot.Controllers;

public class ChoseGroupCallback : AbstractBotCallBack {

    public ChoseGroupCallback() : base("group") {
        
    }

    public override async Task ExecuteAsync(TelegramBotClient botClient, Update update, CancellationToken cancellationToken, string query) {
        Chat chat_id = update.CallbackQuery!.Message!.Chat;
        await botClient.SendMessage(chat_id, query, ParseMode.Markdown);
    }
}
