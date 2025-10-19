using System.Net;
using DiskayBot.API.Clients;
using DiskayBot.API.Exeptions;
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
using DiskayBot.Services.ScheduleService;
using DiskayBot.Services.ScheduleService.Interfaces;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using IScheduleController = DiskayBot.Services.ScheduleService.Interfaces.IScheduleController;

namespace DiskayBot.Bot.Bot;

public class TelegramBot {
    private TelegramBotClient bot;
    private CancellationTokenSource cts_token = new();
    private ILogger<TelegramBot> _logger;
    private ILoggerFactory _loggerFactory;
    
    private readonly CommandRegister _commandRegister;
    private readonly EventRegister _eventRegister;
    private readonly EventCreator _eventCreator;
    public TelegramBot(string botToken, RedisController redis, MemoryController memoryController, IScheduleController scheduleController, 
        ILogger<TelegramBot> logger, ILoggerFactory loggerFactory) {
        
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        
        _eventCreator = new EventCreator();
        _eventRegister = new EventRegister();
        
        var commands = new List<ICommand>() {
            new StartCommand("/start"),
            new CheckStatusCommand("/check_bot_status", memoryController, scheduleController),
            new ShowProfileCommand("/show_profile", memoryController),
            new FastScheduleCommand("/disky", memoryController, scheduleController, "updateSchedule"),
            new CheckSchedulesCommand("/check", memoryController, "checkGroup"),
            new RegisterCommand("/create_account", memoryController, "chooseCourse"),
            new SettingsCommand("/settings", memoryController),
            new AboutCommand("/about", "0.12-alfa"),
            
            new UpdateSchedule("updateSchedule", memoryController, scheduleController),
            
            new ChooseCourseCallBack("chooseCourse", "chooseGroup"),
            new ChooseGroupCallback("chooseGroup", memoryController, redis, _eventRegister, "preCreateAccountOffer", "chooseCourse"),
            new PreCreateAccountOffer("preCreateAccountOffer", redis, "createAccount"),
            new CreatingAccountCallback("createAccount", redis, memoryController),
            
            new ChangeProfileDataCallback("changeProfileData", memoryController),
            new ChooseCourseCallBack("changeCourse", "changeGroup"),
            new ChooseGroupCallback("changeGroup", memoryController, redis, _eventRegister, "changingGroup", "changeCourse"),
            new ChangingGroupCallback("changingGroup", redis, memoryController),
            
            new ChooseGroupCallback("checkGroup", memoryController, redis, _eventRegister, "checkingSchedule", "/check=edit", true, false),
            new CheckScheduleCallback("checkingSchedule", scheduleController, redis, "checkGroup"),
        };
        
        _commandRegister = new CommandRegister(commands);
        
        bot = new TelegramBotClient(botToken);
        bot.OnUpdate += OnUpdate;
    }

    protected async Task OnUpdate(Update update) {
        cts_token.CancelAfter(2000);
        _logger.LogDebug("Обработка запроса");

        var evt = _eventCreator.Create(update);

        _logger.LogInformation($"Запрос от пользователя '{evt.Username}', ID={evt.UserId}");
        _logger.LogInformation($"Тело запроса: {evt.GetContent()}");

        try {
            var @event = _eventRegister.GetEvent(evt.GetContent());

            if (@event != null) {
                _logger.LogDebug($"Обнаружено событие: {@event.Name}");
                await @event.HandleAsync(evt, cts_token.Token);
            }

            var command = _commandRegister.GetCommand(evt.GetContent());

            if (command != null) {
                _logger.LogDebug($"Команда '{command.Name}' найдена, идёт процесс обработки");
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
