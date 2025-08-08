using DiskayBot.Bot.Abstractions;
using DiskayBot.Bot.Messages;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DiskayBot.Bot.Bot.Commands;

public class StartCommand : AbstractBotCommand {
    public StartCommand() : base("/start") {}

    public override async Task ExecuteAsync(TelegramBotClient botClient, Update update, CancellationToken cts_token) {
        var chat_id = update.Message!.Chat;
        await botClient.SendMessage(chat_id, MessageBuilder.StartMessage(), ParseMode.Markdown);
    }
}
