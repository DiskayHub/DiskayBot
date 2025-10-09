using DiskayBot.API.Services;
using DiskayBot.Bot.Abstractions;
using DiskayBot.Bot.Bot.Controllers;
using DiskayBot.Bot.Bot.Exeptions;
using DiskayBot.Bot.Messages;
using DiskayBot.Redis;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using BotCommand = DiskayBot.Bot.Abstractions.BotCommand;

namespace DiskayBot.Bot.Bot.Commands;

public class FastScheduleCommand : BotCommand {
    private readonly UserController _userController;
    private readonly ScheduleService _schedule;
    private readonly string _nextCallback;
    
    public FastScheduleCommand(string name, UserController userController, ScheduleService schedule, string nextCallback) : base(name) {
        _userController = userController;
        _schedule = schedule;
        _nextCallback = nextCallback;
    }

    public override async Task ExecuteAsync(ITelegramBotClient bot, CancellationToken token, UserEvent evt) {
        var user = await _userController.GetUserData(evt.UserId);

        if (user != null) {
            var daySchedule = await _schedule.GetActualSchedule($"ИТ{user.group_name}");
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