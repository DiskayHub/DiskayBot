using DiskayBot.API.Clients;
using DiskayBot.Bot.Abstractions;
using DiskayBot.Bot.Bot.Controllers;
using DiskayBot.Bot.Bot.Exeptions;
using DiskayBot.Bot.Messages;
using DiskayBot.Redis;
using DiskayBot.Services.ScheduleService.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using BotCommand = DiskayBot.Bot.Abstractions.BotCommand;
using IScheduleController = DiskayBot.Services.ScheduleService.Interfaces.IScheduleController;

namespace DiskayBot.Bot.Bot.Commands;

public class FastScheduleCommand : BotCommand {
    private readonly MemoryController _memoryController;
    private readonly IScheduleController _schedule;
    private readonly string _nextCallback;
    
    public FastScheduleCommand(string name, MemoryController memoryController, IScheduleController schedule, string nextCallback) : base(name) {
        _memoryController = memoryController;
        _schedule = schedule;
        _nextCallback = nextCallback;
    }

    public override async Task ExecuteAsync(ITelegramBotClient bot, CancellationToken token, UserEvent evt) {
        var user = await _memoryController.GetUser(evt.UserId);

        if (user != null) {
            var daySchedule = _schedule.GetActualSchedule($"ИТ{user.group_name}");
            if (daySchedule != null) {
                var result = MessageBuilder.ShowSchedule(daySchedule);
                await bot.SendMessage(evt.Chat, result,  ParseMode.Html, replyMarkup: GetInlineKeyboard());
            }
            else {
                await bot.SendMessage(evt.Chat, "Не получилось отправить ближайшее расписание",  ParseMode.Markdown);
            }
        }
        else {
            throw new NotAuthorizatedExeption();   
        }
    }

    public InlineKeyboardMarkup GetInlineKeyboard() {
        var buttons = new InlineKeyboardButton[] {
            InlineKeyboardButton.WithCallbackData("Обновить 💫", _nextCallback)
        };
        return new InlineKeyboardMarkup(buttons);
    }
}