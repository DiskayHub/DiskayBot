using System.Text.RegularExpressions;

namespace DiskayBot.API.Contracts.Schedule;

public class DayScheduleRequest {
    public readonly string Date;
    public readonly string GroupName;

    private DayScheduleRequest(string date, string groupName) {
        Date = date;
        GroupName = groupName;
    }
    
    private static bool CheckValidGroup(string group) {
        return Regex.IsMatch(group, @"^ИТ\d{2}-\d{2}$");
    }
    public static DayScheduleRequest Create(DateOnly date, string groupName) {
        if (CheckValidGroup(groupName)) {
            var dateString = date.ToString("yyyy-MM-dd");
            return new DayScheduleRequest(dateString, groupName);
        }
        throw new ArgumentException("Invalid group name");
    }
}