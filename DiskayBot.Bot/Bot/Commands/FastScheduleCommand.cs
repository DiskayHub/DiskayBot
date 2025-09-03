using DiskayBot.API.Services;
using DiskayBot.Bot.Abstractions;
using DiskayBot.Bot.Messages;
using DiskayBot.Redis;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using BotCommand = DiskayBot.Bot.Abstractions.BotCommand;

namespace DiskayBot.Bot.Bot.Commands;

public class FastScheduleCommand : BotCommand {
    private readonly RedisController _redis;
    private readonly ScheduleService _schedule;
    
    public FastScheduleCommand(RedisController redis, ScheduleService schedule) : base("/fast_schedule") {
        _redis = redis;
        _schedule = schedule;
    }

    public override async Task ExecuteAsync(ITelegramBotClient bot, CancellationToken token, UserEvent evt) {
        try {
            var user = await _redis.GetUser(evt.Username);

            if (user != null) {
                var daySchedule = await _schedule.GetDaySchedule(DateOnly.FromDateTime(DateTime.Now).AddDays(1), $"ИТ{user.group_name}");
                if (daySchedule != null) {
                    var result = MessageBuilder.ShowSchedule(daySchedule);
                    await bot.SendMessage(evt.Chat, result,  ParseMode.Markdown);
                }
                else {
                    await bot.SendMessage(evt.Chat, "Сегодня пар нет :)",  ParseMode.Markdown);
                }
            }
            else {
                await bot.SendMessage(evt.Chat, "Кажется, вы не зарегестрированы", ParseMode.Markdown);
            }
        }
        catch (Exception e) {
            throw new Exception(e.Message);
        }
    }
}