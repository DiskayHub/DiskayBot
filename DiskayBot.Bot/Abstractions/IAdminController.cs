using System;

namespace DiskayBot.Bot.Abstractions;

public interface IAdminController : IUserController {
    string GetAllUsers();
}