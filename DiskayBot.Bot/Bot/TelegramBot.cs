using System.Net;
using DiskayBot.API.Exeptions;
using DiskayBot.API.Services;
using DiskayBot.Bot.Abstractions;
using DiskayBot.Bot.Bot.CallBacks.Account;
using DiskayBot.Bot.Bot.CallBacks.Data;
using DiskayBot.Bot.Bot.CallBacks.Schedule;
using DiskayBot.Bot.Bot.Commands;
using DiskayBot.Bot.Bot.Controllers;
using DiskayBot.Bot.Bot.Exeptions;
using DiskayBot.Bot.Bot.KeyBoard;
using DiskayBot.Bot.Bot.Registers;
using DiskayBot.Bot.Events;
using DiskayBot.Bot.Interfaces;
using DiskayBot.Bot.Messages;
using DiskayBot.Redis;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DiskayBot.Bot.Bot;

public class TelegramBot {
    private TelegramBotClient bot;
    private CancellationTokenSource cts_token = new();
    private ILogger<TelegramBot> _logger;
    private ILoggerFactory _loggerFactory;
    
    private readonly CommandRegister _commandRegister;
    private readonly EventRegister _eventRegister;
    private readonly EventCreator _eventCreator;
    public TelegramBot(string botToken, RedisController redis, UserService userService, ScheduleService scheduleService, 
        ILogger<TelegramBot> logger, ILoggerFactory loggerFactory) {
        
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        
        _eventCreator = new EventCreator();
        _eventRegister = new EventRegister();

        var userController = new UserController(redis, userService);
        
        var commands = new List<ICommand>() {
            new StartCommand("/start"),
            new CheckStatusCommand("/check_bot_status", userService, scheduleService),
            new ShowProfileCommand("/show_profile", userController),
            new FastScheduleCommand("/disky", userController, scheduleService, "updateSchedule"),
            new CheckSchedulesCommand("/check", userController, "checkGroup"),
            new RegisterCommand("/create_account", userController, "chooseCourse"),
            new SettingsCommand("/settings", userController),
            new AboutCommand("/about", "0.1-alfa"),
            
            new UpdateSchedule("updateSchedule", userController, scheduleService),
            
            new ChooseCourseCallBack("chooseCourse", "chooseGroup"),
            new ChooseGroupCallback("chooseGroup", userService, redis, _eventRegister, "preCreateAccountOffer", "chooseCourse"),
            new PreCreateAccountOffer("preCreateAccountOffer", redis, "createAccount"),
            new CreatingAccountCallback("createAccount", redis, userService),
            
            new ChangeProfileDataCallback("changeProfileData", userController),
            new ChooseCourseCallBack("changeCourse", "changeGroup"),
            new ChooseGroupCallback("changeGroup", userService, redis, _eventRegister, "changingGroup", "changeCourse"),
            new ChangingGroupCallback("changingGroup", redis, userController, userService),
            
            new ChooseGroupCallback("checkGroup", userService, redis, _eventRegister, "checkingSchedule", "/check=edit", true),
            new CheckScheduleCallback("checkingSchedule", scheduleService, redis, "checkGroup"),
        };
        
        _commandRegister = new CommandRegister(commands);
        
        bot = new TelegramBotClient(botToken);
        bot.OnUpdate += OnUpdate;
    }

    protected async Task OnUpdate(Update update) {
        cts_token.CancelAfter(2000);
        _logger.LogInformation("Обработка запроса");

        var evt = _eventCreator.Create(update);

        _logger.LogInformation($"Запрос от пользователя '{evt.Username}', ID={evt.UserId}");
        _logger.LogInformation($"Тело запроса: {evt.GetContent()}");

        try {
            var @event = _eventRegister.GetEvent(evt.GetContent());

            if (@event != null) {
                _logger.LogInformation($"Обнаружено событие: {@event.Name}");
                await @event.HandleAsync(evt, cts_token.Token);
            }

            var command = _commandRegister.GetCommand(evt.GetContent());

            if (command != null) {
                _logger.LogInformation($"Команда '{command.Name}' найдена, идёт процесс обработки");
                await command.ExecuteAsync(bot, cts_token.Token, evt);
                _logger.LogInformation("Обработка завершена");
            }
        }
        catch (NotAuthorizatedExeption e) {
            _logger.LogError($"Команда не обработана - пользователь '{evt.Username}' не авторизован");
            _logger.LogDebug("Отправляю сообщение об ошибке");
            await bot.SendMessage(evt.Chat, MessageBuilder.NotRegistered(), ParseMode.Markdown);
        }

        catch (HttpRequestException e) {
            _logger.LogError($"Ошибка при отправке запроса к сервису");
            _logger.LogDebug("Отправляю сообщение об ошибке");
            await bot.SendMessage(evt.Chat, "*Diskay* не может отправить запрос на сервер.", ParseMode.Markdown);
        }

        catch (ConnectionRefuseExeption e) {
            _logger.LogCritical($"Не удаётся подключится к сервису {e.ServiceName}");
            _logger.LogDebug($"Отправляю сообщение об ошибке");
            await bot.SendMessage(evt.Chat, "*Diskay* не может соединится с сервером.", ParseMode.Markdown);
        }
        
        catch (Exception e) {
            _logger.LogCritical(e.Message,
                $"Необработанная ошибка! " +
                $"Произошла при выполнении запроса: {evt.GetContent()} от пользователя '{evt.Username}', ID={evt.UserId}");
            _logger.LogDebug("Отправляю сообщение об ошибке");
            await bot.SendMessage(evt.Chat, "Неизвестная ошибка", ParseMode.Markdown);
        }
    }

    public async Task Start() {
        try{
            _logger.LogInformation("Запуск Telegram бота..");
            var botInfo = await bot.GetMe();
            _logger.LogInformation($"{botInfo.Username} запущен!");
                
            await Task.Delay(Timeout.Infinite);
        }
        catch (Exception ex){
            _logger.LogCritical(ex, "Критическая ошибка при запуске бота");
        }
    }
}
