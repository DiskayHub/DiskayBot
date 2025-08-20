using DiskayBot.Bot.Abstractions;
using DiskayBot.Bot.Events;
using DiskayBot.Bot.Events.Data;
using DiskayBot.Bot.Interfaces;
using DiskayBot.Redis;
using StackExchange.Redis;
using Telegram.Bot;

namespace DiskayBot.Bot.Bot.Events;

public class SaveGroupHandler : EventProcessor {
    private readonly RedisController _redis;
    
    public SaveGroupHandler(string name, RedisController redis) : base(name) {
        _redis = redis;
    }
    public override async Task HandleAsync(UserEvent evt, CancellationToken cancellationToken) {
        var callBackEvent = (CallbackQueryUserEvent)evt;
        
        var cashData = await _redis.GetDataHash(evt.Chat.Id.ToString());

        if (cashData == null){
            if (callBackEvent.Query != String.Empty) {
                var hash = new HashEntry[] {
                    new HashEntry("group_id", callBackEvent.Query),
                };
                await _redis.SaveDataHash(evt.Chat.Id.ToString(), hash, TimeSpan.FromSeconds(40));
            }
            else{
                throw new TimeoutException();
            }
        }
    }
}