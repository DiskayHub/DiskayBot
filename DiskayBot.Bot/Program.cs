using DiskayBot.API.Clients;
using DiskayBot.API.Exeptions;
using DiskayBot.API.Interfaces;
using DiskayBot.Bot;
using DiskayBot.Bot.Bot;
using DiskayBot.Bot.Bot.Controllers;
using DiskayBot.Redis;
using DiskayBot.Services.ScheduleService;
using DotNetEnv;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

var WORK_DIRECTORY = "../../../"; //Путь относительно bin/Debug/net9.0

Env.Load(WORK_DIRECTORY);

string? botToken = Environment.GetEnvironmentVariable("BOT_TOKEN");
bool isDocker = Environment.CurrentDirectory == "/app"; 

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) => {
            if (!isDocker) { // Если бот запущен не в Docker
                Console.WriteLine("DEFAULT CONFIGURATION");
                var path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../"));
                config.SetBasePath(path);   
            }
            else {
                Console.WriteLine("DOCKER CONFIGURATION");
            }
            config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            config.AddEnvironmentVariables();
        }
    )
    .ConfigureServices((context, services) => {
        Console.WriteLine("Текущая директория: " + Environment.CurrentDirectory);
        
        var configurator = new Configurator(context.Configuration, isDocker);
        var configuration = configurator.GetConfiguration();
        
        Console.WriteLine($"REDIS: {configuration.Services.Redis}");
        Console.WriteLine($"DiskayMemory: {configuration.Services.DiskayMemory}");
        Console.WriteLine($"ScheduleService: {configuration.Services.ScheduleService}");
        
        // HttpClient
        services.AddSingleton<HttpClient>();

        // Кеширование - REDIS
        var redis = ConnectionMultiplexer.Connect($"{configuration.Services.Redis.url},abortConnect=false");
        if (redis.IsConnected) {
            services.AddSingleton<RedisController>(sp => 
                new RedisController(redis.GetDatabase(), sp.GetRequiredService<ILogger<RedisController>>())
            );
        }
        else {
            throw new ConnectionRefuseExeption("redis", "Ошибка при подключении к redis");
        }

        // СТОРОННИЕ СЕРВИСЫ

        // DiskayMemory
        services.AddSingleton<UserClient>(sp =>
            new UserClient(
                sp.GetRequiredService<HttpClient>(),
                configuration.Services.DiskayMemory.url,
                "DiskayMemory",
                sp.GetRequiredService<ILogger<UserClient>>()
            )
        );
        services.AddSingleton<MemoryController>(sp => 
            new MemoryController(sp.GetRequiredService<RedisController>(), sp.GetRequiredService<UserClient>())
        );

        // CollegeApi
        services.AddSingleton<IScheduleClient, ScheduleClient>(sp =>
            new ScheduleClient(
                sp.GetRequiredService<HttpClient>(),
                configuration.Services.ScheduleService.url,
                "College"
            )
        );

        // ScheduleService
        services.AddSingleton<ScheduleService>(sp => 
            new ScheduleService(
                client: sp.GetRequiredService<IScheduleClient>(),
                logger: sp.GetRequiredService<ILogger<ScheduleService>>(),
                loggerFactory: sp.GetRequiredService<ILoggerFactory>()
            )
        );

        if (botToken != null) {
            services.AddSingleton<TelegramBot>(sp => {
                    var scheduleService = sp.GetRequiredService<ScheduleService>();
                    return new TelegramBot(
                        botToken,
                        sp.GetRequiredService<RedisController>(),
                        sp.GetRequiredService<MemoryController>(),
                        scheduleService.Controller,
                        sp.GetRequiredService<ILogger<TelegramBot>>(),
                        sp.GetRequiredService<ILoggerFactory>()
                    );
                }
            );
        }
    })
    .ConfigureLogging(logging => {
        logging.ClearProviders();
    })
    .UseSerilog((ctx, lc) => lc
        .MinimumLevel.Debug()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
        )
        .WriteTo.Logger(lc => lc
            .Filter.ByIncludingOnly(log => log.Level >= LogEventLevel.Error)
            .WriteTo.File($"{WORK_DIRECTORY}/logs/errors/errors.log")
        )
        .WriteTo.Logger(lc => lc
            .Filter.ByIncludingOnly(log => log.Level == LogEventLevel.Information)
            .WriteTo.File(
                path: $"{WORK_DIRECTORY}/logs/info/.log",
                rollingInterval: RollingInterval.Day,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
            )
        )

        .WriteTo.File(
            path: $"{WORK_DIRECTORY}/logs/all/.log",
            rollingInterval: RollingInterval.Day,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
        )
    )
    .Build();

if (botToken != null) {
    var bot = host.Services.GetRequiredService<TelegramBot>();
    var scheduleService = host.Services.GetRequiredService<ScheduleService>();
    
    var botThread = new Thread(async void () => {
        await bot.Start();
    });
    var scheduleThread = new Thread(async void () => {
        await scheduleService.Run(TimeSpan.FromMinutes(1));
    });
    
    scheduleThread.Start();
    botThread.Start();
    
    await Task.Delay(Timeout.Infinite);
}
else {
    Console.WriteLine("Токен отсутствует");
}