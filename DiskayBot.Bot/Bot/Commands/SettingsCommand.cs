using DiskayBot.Bot.Abstractions;
using DiskayBot.Redis;
using StackExchange.Redis;
using Telegram.Bot;

namespace DiskayBot.Bot.Bot.Commands;

public class SettingsCommand : BotCommand {
    private readonly RedisController  _redis;
    
    public SettingsCommand(string name) : base(name) {
        
    }

    public override Task ExecuteAsync(ITelegramBotClient bot, CancellationToken token, UserEvent evt) {
        throw new NotImplementedException();
    }
}