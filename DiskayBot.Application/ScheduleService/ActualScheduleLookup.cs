using DiskayBot.API.Contracts;

namespace DiskayBot.Bot.ScheduleService;

public enum ActualScheduleStatus {
    Found,
    // Неделя загружена, но пар до её конца больше нет
    NoClasses,
    // Неделя не загружена: воркер ещё не отработал или сайт колледжа недоступен
    NotLoaded
}

public sealed record ActualScheduleLookup(ActualScheduleStatus Status, DaySchedule? Schedule) {
    public static ActualScheduleLookup Found(DaySchedule schedule) => new(ActualScheduleStatus.Found, schedule);
    public static ActualScheduleLookup NoClasses() => new(ActualScheduleStatus.NoClasses, null);
    public static ActualScheduleLookup NotLoaded() => new(ActualScheduleStatus.NotLoaded, null);
}
