using System.Net;
using DiskayBot.API.Services;
using DiskayBot.Bot.Bot.Controllers;
using DiskayBot.Bot.Bot.Exeptions;
using DiskayBot.Redis;
using StackExchange.Redis;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DiskayBot.Bot.Bot;

public class TelegramBot {
    private TelegramBotClient bot;
    private CancellationTokenSource cts_token = new();
    private ScheduleService _scheduleService;
    private UserService _userService;
    private RedisController _redis;
    private CommandsController _commands;
    private CallBackController _callbacks;
    
    Chat? extract_Chat(Update update) {
        switch (update.Type) {
            case UpdateType.Message:
                return update.Message?.Chat;
            case UpdateType.CallbackQuery:
                return update?.CallbackQuery?.Message?.Chat;
        }
        return null;
    }

    long? extract_UserId(Update update) {
        switch (update.Type){
            case UpdateType.Message:
                return update.Message?.From.Id;
            case UpdateType.CallbackQuery:
                return update.CallbackQuery?.From.Id;
        }
        return null;
    }

    public TelegramBot(string bot_token, RedisController redis, UserService userService, ScheduleService scheduleService ) {
        _redis = redis;
        _userService = userService;
        _scheduleService = scheduleService;
        
        _commands = new CommandsController(_redis, _userService, scheduleService);
        _callbacks = new  CallBackController(_redis, _userService);
        
        bot = new TelegramBotClient(bot_token);
        bot.OnUpdate += OnUpdate;
    }

    protected async Task OnUpdate(Update update) {
        Chat? chat = extract_Chat(update);
        long? user_id = extract_UserId(update);
        
        
        
        cts_token.CancelAfter(2000);

        try{
            Console.Write("\n- - - ОБРАБОТКА ЗАПРОСА - - -\n");
            Console.WriteLine("Diskay принял сообщение");

            if (update.Type == UpdateType.Message && update.Message != null && update.Message.Text != null){
                string text = update.Message.Text;
                Console.WriteLine($"Хм, интересно, что-же он хотел этим сказать: {text}");
                var command = _commands.GetCommand(text);
                if (command != null){
                    Console.WriteLine("О, я знаю эту команду!");
                    await command.ExecuteAsync(bot, update, cts_token.Token);
                }
                else{
                    Console.WriteLine("Ничё не понял но интересно");
                }
            }

            else if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery?.Data != null){
                string callbackQuery = update.CallbackQuery.Data;

                var parts = callbackQuery.Split("_", 2);

                string callback_name = parts[0];
                string? query = parts[1];

                var callback = _callbacks.GetCallBack(callback_name);
                if (callback != null){

                    await bot.AnswerCallbackQuery(update.CallbackQuery.Id);
                    await callback.ExecuteAsync(bot, update, cts_token.Token, query);
                }

                Console.WriteLine(callbackQuery);
            }

            Console.WriteLine("- - - ЗАПРОС ОБРАБОТАН - - -");
        }

        catch (OperationCanceledException){
            await bot.SendMessage(chat, "Превышение времени запроса ⌛", ParseMode.Markdown);
        }

        catch (ConnectionRefuseExeption e){
            await bot.SendMessage(chat, "Diskay не получил ответа на запрос", ParseMode.Markdown);
        }

        catch (HttpRequestException ex){
            await bot.SendMessage(chat, "Diskay не смог обработать запрос", ParseMode.Markdown);
        }

        catch (NullReferenceException ex){
            await bot.SendMessage(chat, "Diskay не смог отправить сообщение 😔", ParseMode.Markdown);
        }

        catch (Exception ex) {
            Console.WriteLine(ex.GetType());
            Console.WriteLine(ex.Message);
            await bot.SendMessage(chat, "Неизвестная ошибка ☠", ParseMode.Markdown);
        }
    }

    public async Task Start() {
        try{
            var bot_info = await bot.GetMe();
            
            Console.WriteLine(
                $"@{bot_info.Username} вылетел в космос и готов выполнять работу, для завершения нажмите enter");
            Console.ReadLine();
        }
        catch (Exception ex){
            Console.WriteLine("Ошибка при запуске бота: " +  ex.Message);
        }
    }
}
