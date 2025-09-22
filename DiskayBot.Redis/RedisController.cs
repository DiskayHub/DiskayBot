using System.Text.Json;
using DiskayBot.API.Contracts;
using DiskayBot.Redis.Abstractions;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace DiskayBot.Redis;

public class RedisController : IRedisController {
    private readonly IDatabase _redis;
    private readonly ILogger<RedisController> _logger;
    
    public RedisController(IDatabase redis, ILogger<RedisController> logger) {
        _redis = redis;
        _logger = logger;
        
        _logger.LogInformation("RedisController - инициализация");
    }

    public async Task SaveUser(string key, UserData data, TimeSpan timeout) {
        _logger.LogInformation("Сохранение пользователя в кэш..");
        try{
            var jsonString = JsonSerializer.Serialize(data);
            await _redis.StringSetAsync(key, jsonString);
            await _redis.KeyExpireAsync(key, timeout);
            _logger.LogDebug($"Пользователь '{data.username}' сохранён в кэш, его ключ: {key}");
        }
        catch (Exception e){
            _logger.LogCritical(e, "Ошибка сохранения пользователя в кеш!");
            Console.WriteLine(e.Message);
        }
    }

    public async Task<UserData?> GetUser(string key) {
        try {
            _logger.LogInformation("Получение пользователя из кэша..");
            _logger.LogDebug($"Ключ к кэшу: {key}");
            
            var data = await _redis.StringGetAsync(key);
            
            if (!data.IsNullOrEmpty){
                _logger.LogInformation("Данные по ключу найдены");
                _logger.LogDebug("Дессериализация данных в объект UserData");
                var userData = JsonSerializer.Deserialize<UserData>(data);
                if (userData != null) {
                    _logger.LogDebug("Дессериализация данных успешно завершена");
                    _logger.LogInformation($"Получен пользователь: {userData.username}, {userData.group_name}");
                    return userData;   
                }
                return null;
            }
            _logger.LogDebug("Пользователя не было в кэше, возвращаю null");
            return null;
        }
        catch (Exception e){
            throw new Exception(e.Message);
        }
    }

    public async Task DeleteUser(string id) {
        _logger.LogInformation("Удаление пользователя из кэша");
        try {
            await _redis.KeyDeleteAsync(id);
        }
        catch (Exception e){
            throw new Exception(e.Message);
        }
    }

    public async Task SaveDataHash(string key, HashEntry[] hash, TimeSpan timeout) {
        _logger.LogInformation("Сохранение данных в кэше");
        _logger.LogDebug($"Обработка сохранения данных: {hash.ToString()} по ключу - {key}");
        try {
            var datakey = $"{key}:process";
            _logger.LogDebug($"Сохраню данные в кэше по ключу: '{datakey}'");
            
            await _redis.HashSetAsync(datakey, hash);
            await _redis.KeyExpireAsync(datakey, timeout);
            
            _logger.LogInformation($"Данные успешно сохранены в кэше, и будут удалены через {timeout.TotalSeconds} секунд");
        }
        catch (Exception e){
            _logger.LogCritical(e,"Ошибка при сохранении данных!");
            throw new Exception(e.Message);
        }
    }

    public async Task<HashEntry[]?> GetDataHash(string key) {
        try {
            var datakey = $"{key}:process";
            var data = await _redis.HashGetAllAsync(datakey);

            if (data.Length != 0){
                var ttl = await _redis.KeyTimeToLiveAsync(datakey);
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
        var datakey = $"{key}:process";
        _logger.LogInformation($"Идёт процесс удаления данных по ключу: {datakey}");
        try {
            await _redis.KeyDeleteAsync(datakey);
            _logger.LogInformation("Данные из кэша удалены");
        }
        catch (Exception e){
            _logger.LogCritical(e,"Ошибка при удалении данных из кэша!");
            throw new Exception(e.Message);
        }
    }
}