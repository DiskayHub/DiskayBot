using System.Net;
using DiskayBot.Infrastructure.Clients;
using DiskayBot.Infrastructure.Contracts;
using DiskayBot.Infrastructure.Contracts.Groups;
using DiskayBot.Infrastructure.Contracts.Service;
using DiskayBot.Infrastructure.Contracts.Users.GetUser;
using DiskayBot.Infrastructure.Contracts.Users.UpdateUser;
using DiskayBot.Infrastructure.Redis.Abstractions;

namespace DiskayBot.Host.Features.Account;

public class MemoryController {
    private readonly IRedisController _redis;
    private readonly UserClient _userClient;
    
    public MemoryController(IRedisController redis, UserClient userClient) {
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

    public async Task<List<TelegramUser>?> GetAllUsers() {
        return await _userClient.GetAllUsers();
    }

    public async Task<List<TelegramUser>?> GetNotifyUsers() {
        return await _userClient.GetNotifyUsers();
    }
}