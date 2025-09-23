using DiskayBot.API.Services;
using DiskayBot.Bot.Abstractions;
using DiskayBot.Bot.Bot.Exeptions;
using Telegram.Bot;

namespace DiskayBot.Bot.Bot.Commands;

public class StatisticCommand : BotCommand {
    private readonly UserService _userService;
    private readonly ScheduleService _scheduleService;
    
    public StatisticCommand(string name, UserService userService, ScheduleService scheduleService) : base(name) {
        _userService = userService;
        _scheduleService = scheduleService;
    }

    public override async Task ExecuteAsync(ITelegramBotClient bot, CancellationToken token, UserEvent evt) {
        var user = await _userService.Authorization(evt.UserId);
        if (user != null) {
            
        }
        else {
            throw new NotAuthorizatedExeption();
        }
    }
}