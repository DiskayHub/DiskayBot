using System;
using DiskayBot.Bot.Abstractions;
using DiskayBot.Bot.Messages;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DiskayBot.Bot.Controllers;

public class BasicController : BotController, IBasicController {
    
    public BasicController(TelegramBotClient botClient, long user_id) : base(botClient, user_id) {}

    public async override Task ProcessMessage(Message message) {
        switch(message.Text) {
            case "/start":
                await BotClient.SendMessage(message.Chat, StartMessage(), ParseMode.Markdown);
                break;
            case "/bot_info":
                await BotClient.SendMessage(message.Chat, BotInfo(), ParseMode.Markdown);
                break;
            case "/register":
                await BotClient.SendMessage(message.Chat, Register(), ParseMode.Markdown);
                break;
            default:
                await BotClient.SendMessage(message.Chat, "Сорри не понял", ParseMode.Markdown);
                break;
        }
    }

    public string StartMessage() {
        return BotMessages.StartMessage();
    }

    public string BotInfo() {
        return "Я дискай";
    }

    public string Register() {
        return "Завтра зарегаешься";
    }
}
