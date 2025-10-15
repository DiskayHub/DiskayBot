using DiskayBot.Bot.Abstractions;
using DiskayBot.Bot.Bot.Controllers;
using DiskayBot.Bot.Messages;
using DiskayBot.Services.ScheduleService.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace DiskayBot.Bot.Bot.Commands;

public class CheckStatusCommand : BotCommand {
    private readonly MemoryController _memoryService;
    private readonly IScheduleController  _scheduleClient;
    
    public CheckStatusCommand(string name, MemoryController memoryService, IScheduleController scheduleClient) : base(name) {
        _memoryService = memoryService;
        _scheduleClient = scheduleClient;
    }

    public override async Task ExecuteAsync(ITelegramBotClient bot, CancellationToken token, UserEvent evt) {
        var userService = await _memoryService.PingService();
        await bot.SendMessage(evt.Chat, MessageBuilder.CheckBotStatus([userService]), ParseMode.Markdown);
    }
}