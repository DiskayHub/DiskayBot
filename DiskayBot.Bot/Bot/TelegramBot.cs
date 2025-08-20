using System.Net;
using DiskayBot.API.Services;
using DiskayBot.Bot.Abstractions;
using DiskayBot.Bot.Bot.CallBacks.Data;
using DiskayBot.Bot.Bot.Commands;
using DiskayBot.Bot.Bot.Events;
using DiskayBot.Bot.Bot.Exeptions;
using DiskayBot.Bot.Bot.KeyBoard;
using DiskayBot.Bot.Bot.Registers;
using DiskayBot.Bot.Events;
using DiskayBot.Bot.Events.Base;
using DiskayBot.Bot.Interfaces;
using DiskayBot.Bot.Messages;
using DiskayBot.Redis;
using StackExchange.Redis;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DiskayBot.Bot.Bot;

public class TelegramBot {
    private TelegramBotClient bot;
    private CancellationTokenSource cts_token = new();
    
    private readonly ScheduleService _scheduleService;
    private readonly UserService _userService;
    private readonly RedisController _redis;
    
    private readonly CommandRegister _commandRegister;
    private readonly EventRegister _eventRegister;
    
    private readonly EventCreator _eventCreator;
    private readonly KeyboardHandler _keyboardHandler;
    public TelegramBot(string bot_token, RedisController redis, UserService userService, ScheduleService scheduleService) {
        _redis = redis;
        _userService = userService;
        _scheduleService = scheduleService;
        _eventCreator = new EventCreator();
        
        _keyboardHandler = new KeyboardHandler();
        
        var commands = new List<ICommand>() {
            new StartCommand("/start"),
            new CheckStatusCommand("/check_bot_status", userService, scheduleService),
            new ShowProfileCommand("/show_profile", redis, userService),
            new RegisterCommand("/create_account", redis, userService, _keyboardHandler)
        };

        var eventHandlers = new List<EventProcessor>() {
            new SaveGroupHandler("group", redis)
        };
        
        _commandRegister = new CommandRegister(commands);
        _eventRegister = new EventRegister(eventHandlers);
        
        bot = new TelegramBotClient(bot_token);
        bot.OnUpdate += OnUpdate;
    }

    protected async Task OnUpdate(Update update) {
        try{
            cts_token.CancelAfter(2000);
            Console.Write("\n- - - ОБРАБОТКА ЗАПРОСА - - -\n");

            var evt = _eventCreator.Create(update);

            Console.WriteLine($"Diskay принял сообщение: {evt.GetContent()}");

            var command = _commandRegister.GetCommand(evt.GetContent());
            var @event = _eventRegister.GetEvent(evt.GetContent());
            var keyboard = _keyboardHandler.GetKeyBoard(evt.GetContent());
            
            if (@event != null){
                await @event.HandleAsync(evt, cts_token.Token);
            }

            if (command != null){
                await command.ExecuteAsync(bot, cts_token.Token, evt);
            }
        }
        catch (Exception e) {
            
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
