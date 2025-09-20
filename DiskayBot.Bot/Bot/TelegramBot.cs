using System.Net;
using DiskayBot.API.Services;
using DiskayBot.Bot.Abstractions;
using DiskayBot.Bot.Bot.CallBacks.Account;
using DiskayBot.Bot.Bot.CallBacks.Data;
using DiskayBot.Bot.Bot.Commands;
using DiskayBot.Bot.Bot.Controllers;
using DiskayBot.Bot.Bot.Exeptions;
using DiskayBot.Bot.Bot.KeyBoard;
using DiskayBot.Bot.Bot.Registers;
using DiskayBot.Bot.Events;
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
    public TelegramBot(string bot_token, RedisController redis, UserService userService, ScheduleService scheduleService) {
        _redis = redis;
        _userService = userService;
        _scheduleService = scheduleService;
        
        _eventCreator = new EventCreator();
        _eventRegister = new EventRegister();

        var userController = new UserController(redis, userService);
        
        var commands = new List<ICommand>() {
            new StartCommand("/start"),
            new CheckStatusCommand("/check_bot_status", userService, scheduleService),
            new ShowProfileCommand("/show_profile", userController),
            new FastScheduleCommand("/disky", userController, scheduleService),
            new RegisterCommand("/create_account", userController, "chooseCourse"),
            new SettingsCommand("/settings", userController),
            
            new ChooseCourseCallBack("chooseCourse", "chooseGroup"),
            new ChooseGroupCallback("chooseGroup", _userService, _redis, _eventRegister, "preCreateAccountOffer", "chooseCourse"),
            new PreCreateAccountOffer("preCreateAccountOffer", redis, "createAccount"),
            new CreatingAccountCallback("createAccount", redis, userService),
            
            new ChangeProfileDataCallback("changeProfileData", userController),
            new ChooseCourseCallBack("changeCourse", "changeGroup"),
            new ChooseGroupCallback("changeGroup", _userService, _redis, _eventRegister, "changingGroup", "changeCourse"),
            new ChangingGroupCallback("changingGroup", redis, userController, userService)
        };
        
        _commandRegister = new CommandRegister(commands);
        
        bot = new TelegramBotClient(bot_token);
        bot.OnUpdate += OnUpdate;
    }

    protected async Task OnUpdate(Update update) {
        cts_token.CancelAfter(2000);
        Console.Write("\n- - - ОБРАБОТКА ЗАПРОСА - - -\n");

        var evt = _eventCreator.Create(update);

        Console.WriteLine($"Diskay принял сообщение: {evt.GetContent()}");
        
        try {
            var @event = _eventRegister.GetEvent(evt.GetContent());

            if (@event != null) {
                Console.WriteLine($"НАЙДЕН ОБРАБОТЧИК СОБЫТИЯ: {@event.Name}");
                await @event.HandleAsync(evt, cts_token.Token);
            }

            var command = _commandRegister.GetCommand(evt.GetContent());

            if (command != null) {
                await command.ExecuteAsync(bot, cts_token.Token, evt);
            }
        }
        catch (NotAuthorizatedExeption e) {
            await bot.SendMessage(evt.Chat, MessageBuilder.NotRegistered(), ParseMode.Markdown);
        }
        
        catch (Exception e) {
            await bot.SendMessage(evt.Chat, "Неизвестная ошибка", ParseMode.Markdown);
        }
    }

    public async Task Start() {
        try{
            var botInfo = await bot.GetMe();
            
            Console.WriteLine(
                $"@{botInfo.Username} вылетел в космос и готов выполнять работу, для завершения нажмите enter");
            await Task.Delay(Timeout.Infinite);
        }
        catch (Exception ex){
            Console.WriteLine("Ошибка при запуске бота: " +  ex.Message);
        }
    }
}
