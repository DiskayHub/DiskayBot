using DiskayBot.API.Services;
using DiskayBot.Bot.Abstractions;
using DiskayBot.Bot.Messages;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace DiskayBot.Bot.Bot.Commands;

public class CheckStatusCommand : BotCommand {
    private readonly UserService _userService;
    private readonly ScheduleService _scheduleService;
    
    public CheckStatusCommand(string name, UserService userService, ScheduleService scheduleService) : base(name) {
        _userService = userService;
        _scheduleService = scheduleService;
    }

    public override async Task ExecuteAsync(ITelegramBotClient bot, CancellationToken token, UserEvent evt) {
        var userService = await _userService.PingService();
        var scheduleService = await _scheduleService.PingService();
        await bot.SendMessage(evt.Chat, MessageBuilder.CheckBotStatus([userService, scheduleService]), ParseMode.Markdown);
    }
}