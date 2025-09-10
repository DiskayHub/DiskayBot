using System.Net;
using DiskayBot.API.Services;
using DiskayBot.Bot.Abstractions;
using DiskayBot.Bot.Events;
using DiskayBot.Redis;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using BotCommand = DiskayBot.Bot.Abstractions.BotCommand;

namespace DiskayBot.Bot.Bot.CallBacks.Account;

public class CreatingAccountCallback : BotCommand {
    private readonly RedisController _redis;
    private readonly UserService _service;

    public CreatingAccountCallback(string callback, RedisController redis, UserService service) : base(callback) {
        _redis = redis;
        _service = service;
    }

    public override async Task ExecuteAsync(ITelegramBotClient bot, CancellationToken token, UserEvent evt) {
        var callBackEvent = (CallbackQueryUserEvent)evt;
            
        await bot.DeleteMessage(evt.Chat, evt.MessageId);

        if (callBackEvent.Query == "yes"){
            
                var cash = await _redis.GetDataHash(evt.Chat.Id.ToString());
            
                if (cash != null){
                    var groupId = cash.FirstOrDefault(x => x.Name.ToString() == "group_id").Value;
                
                    var request = await _service.Registration(evt.UserId, evt.Username, groupId.ToString());
                    if (request == HttpStatusCode.OK){
                        await bot.SendMessage(evt.Chat, $"Добро пожаловать, {evt.Username}!", ParseMode.Markdown);
                        await _redis.DeleteData(evt.Chat.Id.ToString());
                    }
                    else {
                        await bot.SendMessage(evt.Chat, $"Diskay не может запомнить вас :(", ParseMode.Markdown);
                    }
                }
                else {
                    throw new TimeoutException();
                }
        }
        else {
            await bot.SendMessage(evt.Chat, "Операция была отклонена. ", ParseMode.Markdown);
            await _redis.DeleteData(evt.Chat.Id.ToString());
        }
    }
}