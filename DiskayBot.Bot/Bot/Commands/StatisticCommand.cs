using DiskayBot.Bot.Abstractions;
using DiskayBot.Bot.Bot.Controllers;
using DiskayBot.Bot.Bot.Exeptions;
using DiskayBot.Services.ScheduleService;
using Telegram.Bot;

namespace DiskayBot.Bot.Bot.Commands;

public class StatisticCommand : BotCommand {
    private readonly MemoryController _memoryController;
    private readonly ScheduleService _scheduleService;
    
    public StatisticCommand(string name, MemoryController memoryController, ScheduleService scheduleService) : base(name) {
        _memoryController = memoryController;
        _scheduleService = scheduleService;
    }

    public override async Task ExecuteAsync(ITelegramBotClient bot, CancellationToken token, UserEvent evt) {
        throw new NotImplementedException();
    }
}