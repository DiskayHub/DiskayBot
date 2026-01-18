using System.Net;
using DiskayBot.API.Clients;
using DiskayBot.API.Contracts;
using DiskayBot.API.Contracts.Groups;
using DiskayBot.API.Contracts.Service;
using DiskayBot.API.Contracts.Users.UpdateUser;
using DiskayBot.Redis;
using Telegram.Bot.Types;

namespace DiskayBot.Bot.Bot.Controllers;

public class MemoryController {
    private readonly RedisController _redis;
    private readonly UserClient _userClient;
    
    public MemoryController(RedisController redis, UserClient userClient) {
        _redis = redis;
        _userClient = userClient;
    }

    private async Task<UserData?> FindUserAsync(long userId) {
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
    
    public async Task<bool> UserIsAuthenticated(long userId) {
        return await FindUserAsync(userId) != null;
    }
    
    public async Task<bool> CreateUser(long userId, string userName, string groupId) {
        var response = await _userClient.Registration(userId, userName, groupId);
        return response == HttpStatusCode.OK;
    }
    
    public async Task<UserData?> GetUser(long userId) {
        return await FindUserAsync(userId);
    }
    
    public async Task<HttpStatusCode> UpdateUser(long userId, UpdateUserRequest requestBody) {
        return await _userClient.UpdateUser(userId, requestBody);
    }

    public async Task<List<GroupResponse>?> GetCourseGroups(int course) {
        return await _userClient.GetCourseGroups(course);
    }

    public async Task<PingResponse?> PingService() {
        return await _userClient.PingService();
    }
}