using DiskayBot.API.Contracts;
using DiskayBot.API.Modules;

namespace DiskayBot.Services.ScheduleService.Data;

public class WeekSchedule {
    public TimePeriod? WeekPeriod;
    public Dictionary<string, List<DaySchedule>> GroupsSchedule;

    public WeekSchedule() {
        WeekPeriod = null;
        GroupsSchedule = new Dictionary<string, List<DaySchedule>>();
    }

    public WeekSchedule(WeekSchedule other) {
        WeekPeriod = other.WeekPeriod;
        GroupsSchedule = GroupsSchedule = new Dictionary<string, List<DaySchedule>>(other.GroupsSchedule
            .ToDictionary(x => x.Key, x => new List<DaySchedule>(x.Value)));;
    }

    public WeekSchedule(TimePeriod weekPeriod, Dictionary<string, List<DaySchedule>> groupsSchedule) {
        WeekPeriod = weekPeriod;
        GroupsSchedule = groupsSchedule;
    }
};