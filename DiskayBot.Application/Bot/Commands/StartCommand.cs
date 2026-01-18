using DiskayBot.Bot.Abstractions;
using DiskayBot.Bot.Attributes;
using DiskayBot.Bot.DTOs;
using DiskayBot.Bot.Events;
using DiskayBot.Bot.Interfaces;
using DiskayBot.Bot.Messages;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace DiskayBot.Bot.Bot.Commands;

[CommandName("/start")]
public class StartCommand : IBaseCommand {
    public async Task ExecuteAsync(ITelegramBotClient bot, CancellationToken token, UserEvent evt) {
        await bot.SendMessage(evt.Chat, MessageBuilder.StartMessage(), ParseMode.Markdown);
    }
}