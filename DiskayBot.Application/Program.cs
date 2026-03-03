using System.Reflection;
using DiskayBot.API.Clients;
using DiskayBot.API.Clients.Options;
using DiskayBot.API.Interfaces;
using DiskayBot.Bot.Bot;
using DiskayBot.Bot.Bot.Commands.Base;
using DiskayBot.Bot.Bot.Controllers;
using DiskayBot.Bot.Bot.Options;
using DiskayBot.Bot.Bot.Registers;
using DiskayBot.Bot.Extensions;
using DiskayBot.Bot.Middleware;
using DiskayBot.Bot.ScheduleService;
using DiskayBot.Bot.ScheduleService.Options;
using DiskayBot.Bot.ScheduleService.Worker;
using DiskayBot.Redis;
using DiskayBot.Redis.Abstractions;
using DotNetEnv;
using MediatR;
using StackExchange.Redis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var WORK_DIRECTORY = "../../../"; //Путь относительно bin/Debug/net9.0

Env.Load(WORK_DIRECTORY);

bool isDocker = Environment.CurrentDirectory == "/app";

var host = Host.CreateDefaultBuilder(args)
    .AddConfiguration(isDocker)
    .ConfigureServices((context, services) => {
        Console.WriteLine("Текущая директория: " + Environment.CurrentDirectory);

        var configuration = context.Configuration;

        Console.WriteLine($"REDIS: {configuration["Redis:ConnectionString"]}");
        Console.WriteLine($"DiskayMemory: {configuration["UserClient:url"]}");
        Console.WriteLine($"ScheduleBackgroundService: {configuration["ScheduleClient:url"]}");
        
        services.Configure<TelegramBotOptions>(configuration.GetSection("TelegramBot"));
        services.Configure<UserClientOptions>(configuration.GetSection("UserClient"));
        services.Configure<ScheduleClientOptions>(configuration.GetSection("ScheduleClient"));
        services.Configure<ScheduleServiceOptions>(configuration.GetSection("ScheduleService"));
        
        // HttpClient
        services.AddHttpClient();

        // Кеширование - REDIS
        var redisConnectionString = configuration["Redis:ConnectionString"]
            ?? throw new Exception("Redis ConnectionString не настроена");
        
        var multiplexer = ConnectionMultiplexer.Connect(redisConnectionString);
        services.AddSingleton<IConnectionMultiplexer>(multiplexer);
        services.AddSingleton<IRedisController, RedisController>();

        // СТОРОННИЕ СЕРВИСЫ

        // DiskayMemory
        services.AddSingleton<UserClient>();
        services.AddSingleton<MemoryController>();

        // Schedule
        services.AddSingleton<IScheduleClient, ScheduleClient>();
        services.AddSingleton<IScheduleController, ScheduleController>();

        // Сканирование и регистрация команд/каллбеков
        var descriptors = CommandScanner.Scan(Assembly.GetExecutingAssembly()).ToList();

        var commandTypes = descriptors.Select(d => d.CommandType).Distinct();
        foreach (var type in commandTypes) {
            services.AddTransient(type);
        }

        services.AddSingleton(new CommandRegistry(descriptors));
        services.AddSingleton<CommandDispatcher>();
        services.AddSingleton<BotMiddleware>();

        services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });

        services.AddHostedService<TelegramBot>();
        services.AddHostedService<ScheduleBackgroundService>();
    })
    .AddLogging(WORK_DIRECTORY)
    .Build();

await host.RunAsync();