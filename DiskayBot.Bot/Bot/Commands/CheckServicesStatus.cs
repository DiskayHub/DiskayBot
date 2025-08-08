using DiskayBot.API.Services;
using DiskayBot.Bot.Abstractions;
using DiskayBot.Bot.Messages;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DiskayBot.Bot.Bot.Commands;

public class CheckServicesStatus : AbstractBotCommand {
    private readonly UserService _userService;
    private readonly ScheduleService _scheduleService;
    
    public CheckServicesStatus(UserService userService, ScheduleService scheduleService) : base("/check_bot_status") {
        _userService = userService;
        _scheduleService = scheduleService;
    }

    public override async Task ExecuteAsync(TelegramBotClient botClient, Update update, CancellationToken cancellationToken) {
        try{
            var chat = update.Message.Chat;
            var userService = await _userService.PingService();
            var scheduleService = await _scheduleService.PingService();
            await botClient.SendMessage(chat, MessageBuilder.CheckBotStatus([userService, scheduleService]), ParseMode.Markdown);
        }
        catch (Exception e){
            throw new Exception(e.Message);
        }
    }
}