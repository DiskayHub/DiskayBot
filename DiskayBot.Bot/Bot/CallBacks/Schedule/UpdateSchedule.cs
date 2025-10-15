using DiskayBot.API.Clients;
using DiskayBot.Bot.Abstractions;
using DiskayBot.Bot.Bot.Controllers;
using DiskayBot.Bot.Bot.Exeptions;
using DiskayBot.Bot.Events;
using DiskayBot.Bot.Messages;
using DiskayBot.Services.ScheduleService.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DiskayBot.Bot.Bot.CallBacks.Schedule;

public class UpdateSchedule : BotCommand {
    private MemoryController _memoryController;
    private IScheduleController _scheduleService;
    
    public UpdateSchedule(string name, MemoryController memoryController, IScheduleController scheduleService) : base(name) {
        _memoryController =  memoryController;
        _scheduleService = scheduleService;
    }

    public override async Task ExecuteAsync(ITelegramBotClient bot, CancellationToken token, UserEvent evt) {
        var callbackEvent = (CallbackQueryUserEvent)evt;
        var user = await _memoryController.GetUser(evt.Chat.Id);
        if (user != null) {
            var schedule = _scheduleService.GetActualSchedule($"ИТ{user.group_name}");
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