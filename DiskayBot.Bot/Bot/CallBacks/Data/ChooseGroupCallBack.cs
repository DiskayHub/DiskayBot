using System.Data;
using DiskayBot.API.Services;
using DiskayBot.Bot.Abstractions;
using DiskayBot.Bot.Bot.Events;
using DiskayBot.Bot.Bot.Registers;
using DiskayBot.Bot.Events;
using DiskayBot.Bot.Events.Data;
using DiskayBot.Redis;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using BotCommand = DiskayBot.Bot.Abstractions.BotCommand;

namespace DiskayBot.Bot.Bot.CallBacks.Data;

public class ChooseGroupCallback : BotCommand {
    private readonly EventRegister _eventRegister;

    public ChooseGroupCallback(EventRegister eventRegister) : base("chooseGroup") {
        _eventRegister = eventRegister;
    }
    
    public override async Task ExecuteAsync(ITelegramBotClient bot, CancellationToken token, UserEvent evt) {
        var callBackEvent = (CallbackQueryUserEvent)evt;
        await _eventRegister.ShowGroupsHandler(new ShowGroupsEvent(
            bot,
            callBackEvent,
            short.Parse(callBackEvent.Query),
            "Выберите группу",
            "group"
        ), token);
    }
}