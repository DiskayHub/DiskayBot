using DiskayBot.API.Services;
using DiskayBot.Bot.Bot;
using DiskayBot.Redis;
using DotNetEnv;
using StackExchange.Redis;

Env.Load();

string? bot_token = Environment.GetEnvironmentVariable("BOT_TOKEN");
var redis = await ConnectionMultiplexer.ConnectAsync("localhost:6379");
var httpClient = new HttpClient();
var userService = new UserService(httpClient, "http://localhost:5014", "DiskayMemory");
var scheduleService = new ScheduleService(httpClient, "http://localhost:5171", "DiskayCollector");

if (bot_token != null) {
    var bot = new TelegramBot(bot_token, new RedisController(redis.GetDatabase()), userService, scheduleService);
    await bot.Start();
}
else {
    Console.WriteLine("Токен отсутствует");
}