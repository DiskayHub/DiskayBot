using DiskayBot.API.Services;
using DiskayBot.Bot.Bot;
using DiskayBot.Bot.Events;
using DiskayBot.Redis;
using DotNetEnv;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Microsoft.Extensions.Hosting;

Env.Load("/home/laxerem/Documents/my_projects/DiskayHub/DiskayBot/DiskayBot.Bot/");

string? botToken = Environment.GetEnvironmentVariable("BOT_TOKEN");

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) => {
        
        // Кеширование - REDIS
        var redis = ConnectionMultiplexer.Connect("localhost:6379");
        services.AddSingleton(redis.GetDatabase());
        
        // HttpClient
        services.AddSingleton<HttpClient>();
        
        // СТОРОННИЕ СЕРВИСЫ
        
        // DiskayMemory
        services.AddSingleton<UserService>(sp => 
            new UserService(
                sp.GetRequiredService<HttpClient>(),
                "http://localhost:5014", 
                "DiskayMemory"
            )
        );
        
        // DiskayCollector

        services.AddSingleton<ScheduleService>(sp =>
            new ScheduleService(
                sp.GetRequiredService<HttpClient>(),
                "http://localhost:5171", 
                "DiskayCollector"
            )
        );

        if (botToken != null){
            services.AddSingleton<TelegramBot>(sp =>
                new TelegramBot(
                    botToken,
                    new RedisController(sp.GetRequiredService<IDatabase>()),
                    sp.GetRequiredService<UserService>(),
                    sp.GetRequiredService<ScheduleService>()
                )
            );
        }
        
    })
    .Build();

if (botToken != null) {
    var bot = host.Services.GetRequiredService<TelegramBot>();
    await bot.Start();
}
else {
    Console.WriteLine("Токен отсутствует");
}