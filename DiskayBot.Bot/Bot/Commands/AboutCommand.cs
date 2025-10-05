using DiskayBot.Bot.Abstractions;
using DiskayBot.Bot.Messages;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace DiskayBot.Bot.Bot.Commands;

public class AboutCommand : BotCommand {
    private readonly string Version;
    
    public AboutCommand(string name, string version) : base(name) {
        Version = version;
    }

    public override async Task ExecuteAsync(ITelegramBotClient bot, CancellationToken token, UserEvent evt) {
        await bot.SendMessage(evt.Chat, MessageBuilder.AboutBot(Version), ParseMode.Markdown);
    }
}