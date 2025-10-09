using DiskayBot.API.Services;
using DiskayBot.Bot.Abstractions;
using DiskayBot.Bot.Bot.Controllers;
using DiskayBot.Bot.Bot.Exeptions;
using DiskayBot.Bot.Events;
using DiskayBot.Bot.Messages;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DiskayBot.Bot.Bot.CallBacks.Schedule;

public class UpdateSchedule : BotCommand {
    private UserController _userController;
    private ScheduleService _scheduleService;
    
    public UpdateSchedule(string name, UserController userController, ScheduleService scheduleService) : base(name) {
        _userController =  userController;
        _scheduleService = scheduleService;
    }

    public override async Task ExecuteAsync(ITelegramBotClient bot, CancellationToken token, UserEvent evt) {
        var callbackEvent = (CallbackQueryUserEvent)evt;
        var user = await _userController.GetUserData(evt.Chat.Id);
        if (user != null) {
            var schedule = await _scheduleService.GetActualSchedule($"ИТ{user.group_name}");
            if (schedule != null) {
                try {
                    await bot.EditMessageText(
                        evt.Chat,
                        evt.MessageId,
                        MessageBuilder.ShowSchedule(schedule),
                        replyMarkup: GetInlineKeyboard(),
                        parseMode: ParseMode.Html
                    );
                }
                catch (ApiRequestException e) {
                    await bot.AnswerCallbackQuery(callbackEvent.Id);
                }
            }
        }
        else {
            throw new NotAuthorizatedExeption();
        }
    }
    
    public InlineKeyboardMarkup GetInlineKeyboard() {
        var buttons = new InlineKeyboardButton[] {
            InlineKeyboardButton.WithCallbackData("Обновить 💫", Name)
        };
        return new InlineKeyboardMarkup(buttons);
    }
}