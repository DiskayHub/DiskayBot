using System;
using DiskayBot.Bot.Messages;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DiskayBot.Bot.Controllers;

public abstract class BotController {
    protected readonly TelegramBotClient BotClient;
    protected readonly long UserId;

    public BotController(TelegramBotClient botClient, long user_id) {
        BotClient = botClient;
        UserId = user_id;
    }

    public async virtual Task ProcessMessage(Message message) {
        
    }
}
