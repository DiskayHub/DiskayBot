using System;

namespace DiskayBot.Bot.Abstractions;

public interface IUserController : IBasicController {
    string GetSheduleDay();
}
