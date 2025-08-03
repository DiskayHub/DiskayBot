using DiskayBot.API.Contracts;
using StackExchange.Redis;

namespace DiskayBot.Redis.Abstractions;

public interface IRedisController {
    public Task SaveUser(string username, UserData user);
    public Task<UserData?> GetUser(string id);
    public Task DeleteUser(string id);

    public Task SaveDataHash(string key, HashEntry[] hash, TimeSpan timeout);
    public Task<HashEntry[]?> GetDataHash(string key);
    public Task DeleteData(string key);
}