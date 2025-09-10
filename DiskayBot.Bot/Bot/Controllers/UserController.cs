using DiskayBot.API.Contracts;
using DiskayBot.API.Services;
using DiskayBot.Redis;
using Telegram.Bot.Types;

namespace DiskayBot.Bot.Bot.Controllers;

public class UserController {
    private readonly RedisController _redis;
    private readonly UserService _userService;
    
    public UserController(RedisController redis, UserService userService) {
        _redis = redis;
        _userService = userService;
    }

    private async Task<UserData?> SendRequestAsync(long userId) {
        var userCache =  await _redis.GetUser(userId.ToString());
        if (userCache == null) {
            var userDataBase = await _userService.Authorization(userId);
            if (userDataBase != null) {
                await _redis.SaveUser(userDataBase.username, userDataBase, TimeSpan.FromMinutes(30));
                return userDataBase;
            }
        }
        return userCache;
    }

    public async Task<bool> IsAuthenticated(long userId) {
        return await SendRequestAsync(userId) != null;
    }

    public async Task<UserData?> GetUserData(long userId) {
        return await SendRequestAsync(userId);
    }
}