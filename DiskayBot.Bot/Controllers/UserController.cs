using System;
using DiskayBot.Bot.Abstractions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DiskayBot.Bot.Controllers;

public class UserController : BasicController, IUserController {

    public UserController(TelegramBotClient botClient, long user_id) : base(botClient, user_id) {}

    public async override Task ProcessMessage(Message message) {
        switch(message.Text) {
            case "/shedule":
                await BotClient.SendMessage(message.Chat, "Завтра посмотришь", ParseMode.Markdown);
                break;
            default:
                await base.ProcessMessage(message);
                break;
        }
    }

    public string GetSheduleDay() {
        return "Ой да чувак сам посмотри";
    }
}
