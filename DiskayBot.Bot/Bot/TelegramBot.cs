using System.Net;
using DiskayBot.Bot.Bot.Controllers;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DiskayBot.Bot.Bot;

public class TelegramBot {
    private TelegramBotClient bot;
    private CommandsController _commands = new();
    private CallBackController _callbacks = new();
    private CancellationTokenSource cts_token = new();
    
    Chat? extract_Chat(Update update) {
        switch (update.Type) {
            case UpdateType.Message:
                return update.Message?.Chat;
            case UpdateType.CallbackQuery:
                return update.CallbackQuery.Message?.Chat;
        }
        return null;
    }

    public TelegramBot(string bot_token) {
        bot = new TelegramBotClient(bot_token);
        bot.OnUpdate += OnUpdate;
    }

    protected async Task OnUpdate(Update update) {
        Chat? chat = extract_Chat(update);
        cts_token.CancelAfter(2000);

        
        try {
            Console.Write("\n- - - ОБРАБОТКА ЗАПРОСА - - -\n");
            Console.WriteLine("Diskay принял сообщение");

            if (update.Type == UpdateType.Message && update.Message != null && update.Message.Text != null)
            {
                string text = update.Message.Text;
                Console.WriteLine($"Хм, интересно, что-же он хотел этим сказать: {text}");
                var command = _commands.GetCommand(text);
                if (command != null) {
                    Console.WriteLine("О, я знаю эту команду!");
                    await command.ExecuteAsync(bot, update, cts_token.Token);
                }
                else {
                    Console.WriteLine("Ничё не понял но интересно");
                }
            }

            else if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery?.Data != null) {
                string callbackQuery = update.CallbackQuery.Data;

                var parts = callbackQuery.Split("_", 2);

                string callback_name = parts[0];
                string? query = parts[1];

                var callback = _callbacks.GetCallBack(callback_name);
                if (callback != null) {
                    await callback.ExecuteAsync(bot, update, cts_token.Token, query);
                }

                Console.WriteLine(callbackQuery);
            }

            Console.WriteLine("- - - ЗАПРОС ОБРАБОТАН - - -");
        }

        catch (OperationCanceledException){
            await bot.SendMessage(chat, "Превышение времени запроса", ParseMode.Markdown);
        }

        catch (Exception ex){
            await bot.SendMessage(chat, "Ошибка при запросе", ParseMode.Markdown);
        }
    }

    public async Task Start() {
        var bot_info = await bot.GetMe();
        Console.WriteLine($"@{bot_info.Username} вылетел в космос и готов выполнять работу, для завершения нажмите enter");
        Console.ReadLine();
    }
}
