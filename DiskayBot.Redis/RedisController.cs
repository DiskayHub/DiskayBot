using System.Text.Json;
using DiskayBot.Redis.Abstractions;
using StackExchange.Redis;

namespace DiskayBot.Redis;

public class RedisController : IRedisController {
    private readonly IDatabase _redis;
    
    public RedisController(IDatabase redis) {
        _redis = redis;
    }

    public async Task SaveUser(string id, UserData data) {
        try{
            var jsonString = JsonSerializer.Serialize(data);
            await _redis.StringSetAsync(id, jsonString);
        }
        catch (Exception e){
            Console.WriteLine(e.Message);
        }
    }

    public async Task<UserData?> GetUser(string id) {
        try {
            var data = await _redis.StringGetAsync(id);
            var userData = JsonSerializer.Deserialize<UserData>(data);
            return userData;
        }
        catch (Exception e){
            throw new Exception(e.Message);
        }
    }

    public async Task DeleteUser(string id) {
        try {
            await _redis.KeyDeleteAsync(id);
        }
        catch (Exception e){
            throw new Exception(e.Message);
        }
    }

    public async Task SaveDataHash(string key, HashEntry[] hash, TimeSpan timeout) {
        try{
            await _redis.HashSetAsync(key, hash);
            await _redis.KeyExpireAsync(key, timeout);
        }
        catch (Exception e){
            throw new Exception(e.Message);
        }
    }

    public async Task<HashEntry[]?> GetDataHash(string key) {
        try {
            var data = await _redis.HashGetAllAsync(key);
            
            var ttl = await _redis.KeyTimeToLiveAsync(key);
            Console.WriteLine($"TTL: {ttl?.TotalSeconds}");
            
            return data;
        }
        catch (Exception e){
            Console.WriteLine(e.GetType());
            throw new Exception(e.Message);
        }
    }
}