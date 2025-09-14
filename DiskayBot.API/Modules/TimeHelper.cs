namespace DiskayBot.API.Modules;

public class TimeHelper {
    public static bool IsWorkDay(DateOnly date) {
        var dayOfWeek = (int)date.DayOfWeek;
        
        if (dayOfWeek <= 5) {
            var dateStart = date.AddDays(-dayOfWeek);
            var dateEnd = date.AddDays(6);

            return true;
        }
        return false;
    }
    public static TimePeriod GetWeekPeriod(DateOnly date) {
        var dayOfWeek = (int)date.DayOfWeek;
        
        var dateStart = date.AddDays(-dayOfWeek);
        var dateEnd = dateStart.AddDays(6);
        
        return new TimePeriod(dateStart, dateEnd);
    }

    public static TimePeriod GetNextWeekPeriod() {
        var dateNow =  DateOnly.FromDateTime(DateTime.Now);
        var dayOfWeek = (int)dateNow.DayOfWeek;
        
        var daysToNextWeek = 7 - dayOfWeek;

        var dateStart = dateNow.AddDays(daysToNextWeek);
        var dateEnd = dateStart.AddDays(6);

        var period = new TimePeriod(
            Start: dateStart,
            End: dateEnd
        );
        
        return period;
    }

    public static TimePeriod GetActualWeekPeriod() {
        var dateNow =  DateOnly.FromDateTime(DateTime.Now);
        var dayOfWeek = (int)dateNow.DayOfWeek;

        if (dayOfWeek >= 6) {
            return GetNextWeekPeriod();
        }
        return GetWeekPeriod(dateNow);
    }
}