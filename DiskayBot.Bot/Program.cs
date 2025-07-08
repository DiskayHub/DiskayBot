using DiskayBot.Bot.Bot;
using DotNetEnv;
using Telegram.Bot;

Env.Load();

string? bot_token = Environment.GetEnvironmentVariable("BOT_TOKEN");

if (bot_token != null) {
    var bot = new TelegramBot(bot_token);
    await bot.Start();
}