using System.Text.Json;
using DiskayBot.Application.Interfaces;
using StackExchange.Redis;

namespace DiskayBot.Infrastructure.Redis;

public class CacheService : ICacheService {
    private readonly IDatabase _db;

    public CacheService(IConnectionMultiplexer multiplexer) {
        _db = multiplexer.GetDatabase();
    }

    public Task SetAsync<T>(string key, T value, TimeSpan expiration) where T : class {
        var json = JsonSerializer.Serialize(value);
        return _db.StringSetAsync(key, json, expiration);
    }

    public async Task<T?> GetAsync<T>(string key) where T : class {
        var json = await _db.StringGetAsync(key);
        return json.HasValue ? JsonSerializer.Deserialize<T>(json!) : null;
    }

    public Task SetStringAsync(string key, string value, TimeSpan expiration) =>
        _db.StringSetAsync(key, value, expiration);

    public async Task<string?> GetStringAsync(string key) {
        var value = await _db.StringGetAsync(key);
        return value.HasValue ? value.ToString() : null;
    }

    public Task RemoveAsync(string key) => _db.KeyDeleteAsync(key);

    public async Task HashSetAsync(string key, HashEntry[] entries, TimeSpan expiration) {
        await _db.HashSetAsync(key, entries);
        await _db.KeyExpireAsync(key, expiration);
    }

    public async Task<HashEntry[]?> HashGetAllAsync(string key) {
        var data = await _db.HashGetAllAsync(key);
        return data.Length != 0 ? data : null;
    }

    public Task KeyExpireAsync(string key, TimeSpan expiration) =>
        _db.KeyExpireAsync(key, expiration);
}
