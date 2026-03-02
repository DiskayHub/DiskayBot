using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json;
using DiskayBot.API.Contracts;
using DiskayBot.Redis.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace DiskayBot.Redis;

public class RedisController : IRedisController {
    private readonly IDatabase _redis;
    private readonly ILogger<RedisController> _logger;
    
    public RedisController(IConnectionMultiplexer multiplexer, ILogger<RedisController> logger) {
        _redis = multiplexer.GetDatabase();
        _logger = logger;
        _logger.LogInformation("RedisController - инициализация");
    }

    private string GetScheduleHash(DaySchedule schedule) {
        var freshJsonBytes = JsonSerializer.SerializeToUtf8Bytes(schedule);
        return Convert.ToHexString(SHA256.HashData(freshJsonBytes));
    }

    private string GetScheduleKey(DaySchedule schedule) {
        return schedule.date.ToString("dddd", CultureInfo.InvariantCulture).ToLower();
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
        var dataKey = $"{key}:process";
        _logger.LogInformation($"Идёт процесс удаления данных по ключу: {dataKey}");
        try {
            await _redis.KeyDeleteAsync(dataKey);
            _logger.LogInformation("Данные из кэша удалены");
        }
        catch (Exception e){
            _logger.LogCritical(e,"Ошибка при удалении данных из кэша!");
            throw new Exception(e.Message);
        }
    }
    public async Task SaveSchedule(DaySchedule daySchedule) {
        _logger.LogInformation("Сохранение расписания в кэш..");
        try {
            var json = JsonSerializer.Serialize(daySchedule);
            var dayOfWeek = GetScheduleKey(daySchedule);
            await _redis.StringSetAsync($"{dayOfWeek}:hash", GetScheduleHash(daySchedule), TimeSpan.FromDays(3));
            await _redis.StringSetAsync($"{dayOfWeek}:data", json, TimeSpan.FromDays(3));
            
            _logger.LogDebug($"Расписание для '{dayOfWeek}' сохранено в кэш");
        }
        catch (Exception e) {
            _logger.LogCritical(e, "Ошибка сохранения расписания в кэш!");
            throw new Exception(e.Message);
        }
    }

    public async Task<DaySchedule?> GetSchedule(string dayName) {
        _logger.LogInformation("Получение расписания из кэша..");
        try {
            var key = $"{dayName}:data";
            var data = await _redis.StringGetAsync(key);

            if (data.IsNullOrEmpty) {
                _logger.LogDebug($"Расписание по ключу '{key}' не найдено в кэше");
                return null;
            }

            _logger.LogDebug($"Расписание по ключу '{key}' найдено, десериализация..");
            return JsonSerializer.Deserialize<DaySchedule>(data!);
        }
        catch (Exception e) {
            _logger.LogCritical(e, "Ошибка получения расписания из кэша!");
            throw new Exception(e.Message);
        }
    }

    public async Task<bool> CheckScheduleEquals(DaySchedule daySchedule) {
        var result = await _redis.StringGetAsync($"{GetScheduleKey(daySchedule)}:hash");
        return result == GetScheduleHash(daySchedule);
    }
}