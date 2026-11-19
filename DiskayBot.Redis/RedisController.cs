using System.Security.Cryptography;
using System.Text.Json;
using DiskayBot.API.Contracts;
using DiskayBot.Redis.Abstractions;
using DiskayBot.Redis.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace DiskayBot.Redis;

public class RedisController : IRedisController {
    private readonly IDatabase _redis;
    private readonly ILogger<RedisController> _logger;
    private readonly RedisOptions _options;

    public RedisController(IConnectionMultiplexer multiplexer, IOptions<RedisOptions> options, ILogger<RedisController> logger) {
        _redis = multiplexer.GetDatabase();
        _options = options.Value;
        _logger = logger;
    }

    private string GetScheduleHash(DaySchedule schedule) {
        var freshJsonBytes = JsonSerializer.SerializeToUtf8Bytes(schedule);
        return Convert.ToHexString(SHA256.HashData(freshJsonBytes));
    }

    private string GetDateKey(DaySchedule schedule) {
        return schedule.date.ToString("dd.MM.yyyy");
    }

    public async Task SaveUser(string key, UserData data, TimeSpan timeout) {
        try {
            var jsonString = JsonSerializer.Serialize(data);
            await _redis.StringSetAsync(key, jsonString);
            await _redis.KeyExpireAsync(key, timeout);
        }
        catch (Exception e) {
            // Кэш не обязателен для обработки запроса: пользователь уже получен из DiskayMemory
            _logger.LogWarning(e, "Не удалось сохранить пользователя {UserKey} в кэш", key);
        }
    }

    public async Task<UserData?> GetUser(string key) {
        var data = await _redis.StringGetAsync(key);
        if (data.IsNullOrEmpty) {
            _logger.LogDebug("Пользователь {UserKey} не найден в кэше", key);
            return null;
        }

        return JsonSerializer.Deserialize<UserData>(data!);
    }

    public async Task DeleteUser(string id) {
        await _redis.KeyDeleteAsync(id);
    }

    public async Task SaveDataHash(string key, HashEntry[] hash, TimeSpan timeout) {
        var datakey = $"{key}:process";
        await _redis.HashSetAsync(datakey, hash);
        await _redis.KeyExpireAsync(datakey, timeout);
    }

    public async Task<HashEntry[]?> GetDataHash(string key) {
        var data = await _redis.HashGetAllAsync($"{key}:process");
        return data.Length != 0 ? data : null;
    }

    public async Task DeleteData(string key) {
        await _redis.KeyDeleteAsync($"{key}:process");
    }

    public async Task SaveSchedule(DaySchedule daySchedule) {
        var json = JsonSerializer.Serialize(daySchedule);
        var dateKey = GetDateKey(daySchedule);
        var expire = TimeSpan.FromDays((double)_options.scheduleExpireDays);
        await _redis.StringSetAsync($"{dateKey}:{daySchedule.mainGroup}:hash", GetScheduleHash(daySchedule), expire);
        await _redis.StringSetAsync($"{dateKey}:{daySchedule.mainGroup}:data", json, expire);
    }

    public async Task SetScheduleDefaultExpire(DaySchedule daySchedule) {
        var dataKey = GetDateKey(daySchedule);
        var expire = TimeSpan.FromDays((double)_options.scheduleExpireDays);
        await _redis.KeyExpireAsync($"{dataKey}:{daySchedule.mainGroup}:hash", expire);
        await _redis.KeyExpireAsync($"{dataKey}:{daySchedule.mainGroup}:data", expire);
    }

    public async Task<DaySchedule?> GetSchedule(string groupName, DateOnly date) {
        var data = await _redis.StringGetAsync($"{date:dd.MM.yyyy}:{groupName}:data");
        return data.IsNullOrEmpty ? null : JsonSerializer.Deserialize<DaySchedule>(data!);
    }

    public async Task<bool> CheckScheduleEquals(DaySchedule daySchedule) {
        var dateKey = GetDateKey(daySchedule);
        var result = await _redis.StringGetAsync($"{dateKey}:{daySchedule.mainGroup}:hash");
        return result == GetScheduleHash(daySchedule);
    }
}