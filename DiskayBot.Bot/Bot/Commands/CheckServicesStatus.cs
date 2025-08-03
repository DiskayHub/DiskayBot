using DiskayBot.API.Services;
using DiskayBot.Bot.Abstractions;
using DiskayBot.Bot.Messages;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DiskayBot.Bot.Bot.Commands;

public class CheckServicesStatus : AbstractBotCommand {
    private readonly MemoryService _service;
    
    public CheckServicesStatus(MemoryService service) : base("/check_bot_status") {
        _service = service;
    }

    public override async Task ExecuteAsync(TelegramBotClient botClient, Update update, CancellationToken cancellationToken) {
        try{
            var chat = update.Message.Chat;
            var response = await _service.PingService();
            await botClient.SendMessage(chat, MessageBuilder.CheckBotStatus(response), ParseMode.Markdown);
        }
        catch (Exception e){
            throw new Exception(e.Message);
        }
    }
}