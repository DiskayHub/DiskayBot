using System.Globalization;
using DiskayBot.API.Contracts.Schedule;

namespace DiskayBot.API.Modules;

public class ScheduleFormatter {
    public static List<DaySchedule> FormatPeriod(List<ApiItem> scheduleResponses, string group) {
        var daysResult = new Dictionary<string, DaySchedule>();

        foreach (var apiItem in scheduleResponses) {
            // дата в формате yyyy-MM-dd
            var date = DateOnly.ParseExact(apiItem.Day, "yyyy-MM-dd");

            if (!daysResult.ContainsKey(apiItem.Day)) {
                daysResult[apiItem.Day] = new DaySchedule(date, group, new List<DayItem>());
            }

            // парсим время (формат "2025-09-10 09:00")
            var startTime = TimeOnly.ParseExact(apiItem.start.Split(' ')[1], "HH:mm");
            var endTime   = TimeOnly.ParseExact(apiItem.end.Split(' ')[1], "HH:mm");

            // подгруппы, если есть
            List<SubGroupItem>? subGroups = null;
            if (apiItem.SubGroup != null && apiItem.SubGroup.Count > 0) {
                subGroups = apiItem.SubGroup.Select(subItem =>
                    new SubGroupItem(
                        name: subItem.STitle,
                        description: subItem.STopic,
                        roomName: subItem.SGCaID,
                        subGroup: subItem.SGrID
                    )
                ).ToList();
            }

            // создаём DayItem
            var dayItem = new DayItem(
                name: apiItem.title,
                description: apiItem.topic,
                room_name: string.IsNullOrWhiteSpace(apiItem.room) ? null : apiItem.room,
                startTime: startTime,
                endTime: endTime,
                subGroups: subGroups
            );

            daysResult[apiItem.Day].items.Add(dayItem);
        }

        // отдаём список по возрастанию дат
        return daysResult.Values.OrderBy(d => d.date).ToList();
    }
}
