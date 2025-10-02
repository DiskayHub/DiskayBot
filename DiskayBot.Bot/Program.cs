using DiskayBot.API.Clients;
using DiskayBot.API.Exeptions;
using DiskayBot.API.Services;
using DiskayBot.Bot.Bot;
using DiskayBot.Bot.Bot.Exeptions;
using DiskayBot.Bot.Events;
using DiskayBot.Redis;
using DiskayBot.Services.ScheduleService;
using DotNetEnv;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

var WORK_DIRECTORY = "../../../";

Env.Load(WORK_DIRECTORY);

string? botToken = Environment.GetEnvironmentVariable("BOT_TOKEN");

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) => {
        Console.WriteLine("Текущая директория: " + Environment.CurrentDirectory);

        // Кеширование - REDIS
        var redis = ConnectionMultiplexer.Connect("localhost:6379,abortConnect=false");
        if (redis.IsConnected) {
            services.AddSingleton(redis.GetDatabase());
        }
        else {
            throw new ConnectionRefuseExeption("redis", "Ошибка при подключении к redis");
        }

        // HttpClient
        services.AddSingleton<HttpClient>();

        // СТОРОННИЕ СЕРВИСЫ

        // DiskayMemory
        services.AddSingleton<UserClient>(sp =>
            new UserClient(
                sp.GetRequiredService<HttpClient>(),
                "http://localhost:8080",
                "DiskayMemory",
                sp.GetRequiredService<ILogger<UserClient>>()
            )
        );

        // CollegeApi

        services.AddSingleton<ScheduleClient>(sp =>
            new ScheduleClient(
                sp.GetRequiredService<HttpClient>(),
                "https://portal.it-college.ru",
                "College"
            )
        );

        if (botToken != null) {
            services.AddSingleton<TelegramBot>(sp =>
                new TelegramBot(
                    botToken,
                    new RedisController(sp.GetRequiredService<IDatabase>(),
                        sp.GetRequiredService<ILogger<RedisController>>()),
                    sp.GetRequiredService<UserClient>(),
                    sp.GetRequiredService<ScheduleService>(),
                    sp.GetRequiredService<ILogger<TelegramBot>>(),
                    sp.GetRequiredService<ILoggerFactory>()
                )
            );
        }

        services.AddSingleton<ScheduleService>(sp => 
            new ScheduleService(sp.GetRequiredService<ScheduleClient>(), sp.GetRequiredService<ILogger<ScheduleService>>())
        );
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
            path: $"{WORK_DIRECTORY}/logs/all/.log", // путь относительно bin/Debug/net9.0
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
        await scheduleService.Run(TimeSpan.FromMinutes(10));
    });
    
    scheduleThread.Start();
    botThread.Start();
    
    await Task.Delay(Timeout.Infinite);
}
else {
    Console.WriteLine("Токен отсутствует");
}