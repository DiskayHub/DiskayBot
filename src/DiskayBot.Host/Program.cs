using System.Reflection;
using DiskayBot.Infrastructure.Clients;
using DiskayBot.Infrastructure.Clients.Options;
using DiskayBot.Infrastructure.Interfaces;
using DiskayBot.Host.Features.Account;
using DiskayBot.Host.Features.Admin;
using DiskayBot.Host.Configuration;
using DiskayBot.Host.Features.Schedule;
using DiskayBot.Host.Features.Schedule.Options;
using DiskayBot.Host.Features.Schedule.Workers;
using DiskayBot.Host.Telegram;
using DiskayBot.Host.Telegram.Commands.Base;
using DiskayBot.Host.Telegram.Middleware;
using DiskayBot.Host.Telegram.Registry;
using DiskayBot.Infrastructure.Redis;
using DiskayBot.Infrastructure.Redis.Abstractions;
using DiskayBot.Infrastructure.Redis.Options;
using DotNetEnv;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;

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
        services.Configure<AdminOptions>(configuration.GetSection("Admin"));
        services.Configure<UserClientOptions>(configuration.GetSection("UserClient"));
        services.Configure<ScheduleClientOptions>(configuration.GetSection("ScheduleClient"));
        services.Configure<ScheduleServiceOptions>(configuration.GetSection("ScheduleService"));
        services.Configure<RedisOptions>(configuration.GetSection("Redis"));
        
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
