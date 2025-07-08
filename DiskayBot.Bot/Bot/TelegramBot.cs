using System.Net;
using DiskayBot.API.Services;
using DiskayBot.Bot.Controllers;
using DiskayBot.Bot.Messages;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DiskayBot.Bot.Bot;

public class TelegramBot {
    private TelegramBotClient bot;
    private CancellationTokenSource cts_token = new();

    public TelegramBot(string bot_token) {
        bot = new TelegramBotClient(bot_token);
        bot.OnMessage += OnMessage;
    }

    protected async Task OnMessage(Message msg, UpdateType update) {
        Console.WriteLine("Diskay принял сообщение");
        Console.WriteLine($"Хм, интересно, что-же он хотел этим сказать: {msg.Text}");

        var authorization_request = await BotService.authorization(msg.From.Id);

        if (authorization_request == HttpStatusCode.OK) {
            UserController controller = new(bot, msg.From.Id);
            await controller.ProcessMessage(msg);
        }
        else if (authorization_request == HttpStatusCode.NotFound) {
            BasicController controller = new(bot, msg.From.Id);
            await controller.ProcessMessage(msg);
        }
        
        else {
            await bot.SendMessage(msg.Chat, "Страшная ошибка, попробуйте ещё раз.", ParseMode.Markdown);
        }

        Console.WriteLine("Сообщение обработано");
    }

    public async Task Start() {
        var bot_info = await bot.GetMe();
        Console.WriteLine($"@{bot_info.Username} вылетел в космос и готов выполнять работу, для завершения нажмите enter");
        Console.ReadLine();
    }
}
