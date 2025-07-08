using System;

namespace DiskayBot.Bot.Abstractions;

public interface IBasicController {
    string StartMessage();
    string BotInfo();
    string Register();
}
