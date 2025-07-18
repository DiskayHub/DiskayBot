using System;
using DiskayBot.Bot.Abstractions;
using Telegram.Bot.Types;

namespace DiskayBot.Bot.Bot.Controllers;

public class CallBackController {
    private readonly Dictionary<string, AbstractBotCallBack> _callback;

    public CallBackController() {
        _callback = new Dictionary<string, AbstractBotCallBack> {
            {"group", new ChoseGroupCallback() }
        };
    }

    public ICallBack? GetCallBack(string callback) {
        _callback.TryGetValue(callback, out var result);
        return result;
    }
}