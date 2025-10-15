using DiskayBot.API.Contracts;
using DiskayBot.API.Contracts.Schedule;
using DiskayBot.API.Exeptions;
using DiskayBot.API.Interfaces;
using DiskayBot.API.Modules;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace DiskayBot.API.Clients;

public class ScheduleClient : IScheduleClient {
    private readonly HttpClient _client;
    private readonly string _baseUrl;
    public readonly string Name;
    public ScheduleClient(HttpClient client, string base_url, string name) {
        _client = client;
        _baseUrl = base_url;
        Name = name;
    }

    private async Task<List<ApiItem>?> GetSchedule(DayScheduleRequest requestBody) {
        var response = await _client.PostAsync($"{_baseUrl}/schedule25.php", requestBody.GetStringContent());

        if (response.IsSuccessStatusCode) {
            var content = await response.Content.ReadAsStringAsync();
            var responseObject = JsonSerializer.Deserialize<List<ApiItem>>(content);
            if (responseObject != null && responseObject.Count > 0) {
                return responseObject;
            }
            return null;   
        }

        throw new ConnectionRefuseExeption("CollegeAPI", "Сервер не отвечает, или отвечает, но не успешно");
    }

    public async Task<GroupWeekSchedule?> GetActualScheduleWeek(string groupName) {
        var weekPeriod = TimeHelper.GetActualWeekPeriod();
        var requestBody = DayScheduleRequest.Create(weekPeriod, groupName);

        if (requestBody != null) {
            var responseObject = await GetSchedule(requestBody);
            if (responseObject != null) {
                var days = ScheduleFormatter.FormatPeriod(responseObject, groupName);
                var weekSchedule = new GroupWeekSchedule(
                    WeekPeriod: weekPeriod,
                    Schedule: days
                );
                return weekSchedule;
            }
            return null;
        }
        throw new Exception("Unable to determine schedule period");
    }

    public async Task<DaySchedule?> GetActualSchedule(string groupName) {
        var actualPeriod = TimeHelper.GetActualWeekPeriod();
        var requestBody = DayScheduleRequest.Create(
            dayStart: actualPeriod.Start.ToString("yyyy-MM-dd"),
            dayEnd: actualPeriod.End.ToString("yyyy-MM-dd"),
            groupName
        );

        if (requestBody != null) {
            var responseObject = await GetSchedule(requestBody);
            if (responseObject != null) {
                var days = ScheduleFormatter.FormatPeriod(responseObject, groupName);
                if (days[0].date == DateOnly.FromDateTime(DateTime.Now) &&
                    days[0].items[^1].endTime < TimeOnly.FromDateTime(DateTime.Now)) {
                    return days.Count > 1 ? days[1] : null;
                }
                return days[0];
            }
            return null;
        }
        throw new Exception("Unable to determine schedule period");
    }
}