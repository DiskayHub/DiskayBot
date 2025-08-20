using DiskayBot.Bot.Abstractions;
using DiskayBot.Bot.Events;
using DiskayBot.Bot.Interfaces;
using DiskayBot.Bot.Messages;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace DiskayBot.Bot.Bot.Commands;

public class StartCommand : BotCommand {
    public StartCommand(string name) : base(name) {}

    public override async Task ExecuteAsync(ITelegramBotClient bot, CancellationToken token, UserEvent messageUserEvent) {
        await bot.SendMessage(messageUserEvent.Chat, MessageBuilder.StartMessage(), ParseMode.Markdown);
    }
}