using DiskayBot.API.Contracts;
using StackExchange.Redis;

namespace DiskayBot.Redis.Abstractions;

public interface IRedisController {
    public Task SaveUser(string key, UserData user, TimeSpan timeout);
    public Task<UserData?> GetUser(string key);
    public Task DeleteUser(string id);

    public Task SaveDataHash(string key, HashEntry[] hash, TimeSpan timeout);
    public Task<HashEntry[]?> GetDataHash(string key);
    public Task DeleteData(string key);

    public Task SaveSchedule(DaySchedule daySchedule);
    public Task<DaySchedule?> GetSchedule(string groupName, DateOnly date);
    public Task<bool> CheckScheduleEquals(DaySchedule daySchedule);
    public Task SetScheduleDefaultExpire(DaySchedule daySchedule);

    // Факт загрузки недели: без него отсутствие дня в кэше не отличить «пар нет» от «данные не получены»
    public Task MarkWeekLoaded(string groupName, DateOnly weekStart);
    public Task<bool> IsWeekLoaded(string groupName, DateOnly weekStart);
}