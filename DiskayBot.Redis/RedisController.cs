using System.Text.Json;
using DiskayBot.API.Contracts;
using DiskayBot.Redis.Abstractions;
using StackExchange.Redis;

namespace DiskayBot.Redis;

public class RedisController : IRedisController {
    private readonly IDatabase _redis;
    
    public RedisController(IDatabase redis) {
        _redis = redis;
    }

    public async Task SaveUser(string username, UserData data) {
        try{
            var jsonString = JsonSerializer.Serialize(data);
            await _redis.StringSetAsync(username, jsonString);
        }
        catch (Exception e){
            Console.WriteLine(e.Message);
        }
    }

    public async Task<UserData?> GetUser(string id) {
        try {
            Console.WriteLine("[REDIS]: ПЫТАЮСЬ ПОЛУЧИТЬ ПОЛЬЗОВАТЕЛЯ - " + id);
            
            var data = await _redis.StringGetAsync(id);
            
            if (!data.IsNullOrEmpty){
                var userData = JsonSerializer.Deserialize<UserData>(data);
                return userData;
            }
            
            return null;
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
            Console.WriteLine("[REDIS]: СОХРАНИЛ ИНФОРМАЦИЮ - " + key);
        }
        catch (Exception e){
            throw new Exception(e.Message);
        }
    }

    public async Task<HashEntry[]?> GetDataHash(string key) {
        try {
            var data = await _redis.HashGetAllAsync(key);

            if (data.Length != 0){
                var ttl = await _redis.KeyTimeToLiveAsync(key);
                Console.WriteLine($"TTL: {ttl?.TotalSeconds}");
                return data;
            }
            
            return null;
        }
        catch (Exception e){
            Console.WriteLine(e.GetType());
            throw new Exception(e.Message);
        }
    }

    public async Task DeleteData(string key) {
        try {
            await _redis.KeyDeleteAsync(key);
        }
        catch (Exception e){
            Console.WriteLine(e.GetType());
            throw new Exception(e.Message);
        }
    }
}