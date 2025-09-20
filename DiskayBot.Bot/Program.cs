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
        var redis = ConnectionMultiplexer.Connect("redis:6379,abortConnect=false");
        services.AddSingleton(redis.GetDatabase());
        
        // HttpClient
        services.AddSingleton<HttpClient>();
        
        // СТОРОННИЕ СЕРВИСЫ
        
        // DiskayMemory
        services.AddSingleton<UserService>(sp => 
            new UserService(
                sp.GetRequiredService<HttpClient>(),
                "http://diskay_memory:8080", 
                "DiskayMemory"
            )
        );
        
        // CollegeApi

        services.AddSingleton<ScheduleService>(sp =>
            new ScheduleService(
                sp.GetRequiredService<HttpClient>(),
                "https://portal.it-college.ru", 
                "College"
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