using DiskayBot.Bot.Abstractions;
using DiskayBot.Bot.Attributes;
using DiskayBot.Bot.Interfaces;
using DiskayBot.Bot.Messages;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace DiskayBot.Bot.Bot.Commands;

[CommandName("/about")]
public class AboutCommand : IBaseCommand {
    private readonly string Version;
    
    public AboutCommand(string version) {
        Version = version;
    }

    public async Task ExecuteAsync(ITelegramBotClient bot, CancellationToken token, UserEvent evt) {
        await bot.SendMessage(evt.Chat, MessageBuilder.AboutBot(Version), ParseMode.Markdown);
    }
}