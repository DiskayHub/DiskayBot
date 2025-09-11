using DiskayBot.API.Services;
using DiskayBot.Bot.Abstractions;
using DiskayBot.Bot.Bot.Controllers;
using DiskayBot.Bot.Bot.Exeptions;
using DiskayBot.Bot.Messages;
using DiskayBot.Redis;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using BotCommand = DiskayBot.Bot.Abstractions.BotCommand;

namespace DiskayBot.Bot.Bot.Commands;

public class FastScheduleCommand : BotCommand {
    private readonly UserController _userController;
    private readonly ScheduleService _schedule;
    
    public FastScheduleCommand(UserController userController, ScheduleService schedule) : base("/fast_schedule") {
        _userController = userController;
        _schedule = schedule;
    }

    public override async Task ExecuteAsync(ITelegramBotClient bot, CancellationToken token, UserEvent evt) {
        var user = await _userController.GetUserData(evt.UserId);

        if (user != null) {
            var daySchedule = await _schedule.GetDaySchedule(DateOnly.FromDateTime(DateTime.Now), $"ИТ{user.group_name}");
            if (daySchedule != null) {
                var result = MessageBuilder.ShowSchedule(daySchedule);
                await bot.SendMessage(evt.Chat, result,  ParseMode.Markdown);
            }
            else {
                await bot.SendMessage(evt.Chat, "Сегодня пар нет :)",  ParseMode.Markdown);
            }
        }
        else {
            throw new NotAuthorizatedExeption();   
        }
    }
}