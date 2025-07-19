using System;
using DiskayBot.Bot.Abstractions;
using DiskayBot.Redis;
using Telegram.Bot.Types;

namespace DiskayBot.Bot.Bot.Controllers;

public class CallBackController {
    private readonly Dictionary<string, AbstractBotCallBack> _callback;

    public CallBackController(RedisController redis) {
        _callback = new Dictionary<string, AbstractBotCallBack> {
            {"group", new ChoseGroupCallback(redis) },
            {"createAccount", new CreateAccountCallBack(redis) }
        };
    }

    public ICallBack? GetCallBack(string callback) {
        _callback.TryGetValue(callback, out var result);
        return result;
    }
}