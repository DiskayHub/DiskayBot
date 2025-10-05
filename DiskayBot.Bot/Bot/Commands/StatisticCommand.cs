using DiskayBot.API.Clients;
using DiskayBot.Bot.Abstractions;
using DiskayBot.Bot.Bot.Exeptions;
using Telegram.Bot;

namespace DiskayBot.Bot.Bot.Commands;

public class StatisticCommand : BotCommand {
    private readonly UserClient _userClient;
    private readonly ScheduleClient _scheduleClient;
    
    public StatisticCommand(string name, UserClient userClient, ScheduleClient scheduleClient) : base(name) {
        _userClient = userClient;
        _scheduleClient = scheduleClient;
    }

    public override async Task ExecuteAsync(ITelegramBotClient bot, CancellationToken token, UserEvent evt) {
        var user = await _userClient.Authorization(evt.UserId);
        if (user != null) {
            
        }
        else {
            throw new NotAuthorizatedExeption();
        }
    }
}