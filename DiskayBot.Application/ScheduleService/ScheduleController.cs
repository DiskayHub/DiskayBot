using DiskayBot.API.Contracts;
using DiskayBot.API.Modules;
using DiskayBot.Redis.Abstractions;

namespace DiskayBot.Bot.ScheduleService;

public class ScheduleController : IScheduleController {
    private readonly IRedisController _redis;
    public ScheduleController(IRedisController redis) {
        _redis = redis;
    }

    public async Task<DaySchedule?> GetActualSchedule(string groupName) {
        var period = TimeHelper.GetActualWeekPeriod();
        var startDay = period.Start;
        var today = DateOnly.FromDateTime(DateTime.Now);
        var now = TimeOnly.FromDateTime(DateTime.Now);

        while (startDay != period.End) {
            var daySchedule = await _redis.GetSchedule(groupName, startDay);
            if (daySchedule != null) {
                if (daySchedule.date == today) {
                    var classesFinished = daySchedule.items[daySchedule.items.Count - 1].endTime <= now;
                    if (!classesFinished) {
                        return daySchedule;
                    }
                } else {
                    return daySchedule;
                }
            }
            startDay = startDay.AddDays(1);
        }
        return null;
    }

    public async Task<DaySchedule?> GetNextSchedule(string groupName, DateOnly date) {
        var limit = date.AddDays(14);
        var current = date.AddDays(1);

        while (current <= limit) {
            var daySchedule = await _redis.GetSchedule(groupName, current);
            if (daySchedule != null)
                return daySchedule;
            current = current.AddDays(1);
        }
        return null;
    }

    public async Task<DaySchedule?> GetPreviousSchedule(string groupName, DateOnly date) {
        var limit = date.AddDays(-14);
        var current = date.AddDays(-1);

        while (current >= limit) {
            var daySchedule = await _redis.GetSchedule(groupName, current);
            if (daySchedule != null)
                return daySchedule;
            current = current.AddDays(-1);
        }
        return null;
    }
}