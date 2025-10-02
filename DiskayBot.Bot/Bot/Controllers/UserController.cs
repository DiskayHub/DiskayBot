using DiskayBot.API.Contracts;
using DiskayBot.API.Services;
using DiskayBot.Redis;
using Telegram.Bot.Types;

namespace DiskayBot.Bot.Bot.Controllers;

public class UserController {
    private readonly RedisController _redis;
    private readonly UserClient _userClient;
    
    public UserController(RedisController redis, UserClient userClient) {
        _redis = redis;
        _userClient = userClient;
    }

    private async Task<UserData?> SendRequestAsync(long userId) {
        var userCache =  await _redis.GetUser(userId.ToString());
        if (userCache == null) {
            var userDataBase = await _userClient.Authorization(userId);
            if (userDataBase != null) {
                await _redis.SaveUser(userId.ToString(), userDataBase, TimeSpan.FromMinutes(30));
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