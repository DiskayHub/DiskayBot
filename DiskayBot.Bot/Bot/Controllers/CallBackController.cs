using System;
using DiskayBot.API.Services;
using DiskayBot.Bot.Abstractions;
using DiskayBot.Bot.Bot.CallBackQuery;
using DiskayBot.Redis;
using Telegram.Bot.Types;

namespace DiskayBot.Bot.Bot.Controllers;

public class CallBackController {
    private readonly Dictionary<string, AbstractBotCallBack> _callback;

    public CallBackController(RedisController redis, MemoryService service) {
        _callback = new Dictionary<string, AbstractBotCallBack> {
            {"course", new ChouseCourseCallBack(redis, service)},
            {"group", new ChoseGroupCallback(redis) },
            {"createAccount", new CreateAccountCallBack(redis, service) }
        };
    }

    public ICallBack? GetCallBack(string callback) {
        _callback.TryGetValue(callback, out var result);
        return result;
    }
}