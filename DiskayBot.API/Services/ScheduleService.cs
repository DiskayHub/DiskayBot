using DiskayBot.API.Contracts.Schedule;
using DiskayBot.API.Contracts.Service;
using DiskayBot.API.Modules;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace DiskayBot.API.Services;

public class ScheduleService {
    private readonly HttpClient _client;
    private readonly string _baseUrl;
    public readonly string Name;
    public ScheduleService(HttpClient client, string base_url, string name) {
        _client = client;
        _baseUrl = base_url;
        Name = name;
    }

    public async Task<DaySchedule?> GetActualSchedule(string groupName) {
        try {
            var actualPeriod = TimeHelper.GetActualWeekPeriod();
            var requestBody = DayScheduleRequest.Create(
                dayStart: actualPeriod.Start.ToString("yyyy-MM-dd"),
                dayEnd: actualPeriod.End.ToString("yyyy-MM-dd"),
                groupName
            );

            if (requestBody != null) {
                var response = await _client.PostAsync($"{_baseUrl}/schedule25.php", requestBody.GetStringContent());
                if (response.IsSuccessStatusCode) {
                    var content = await response.Content.ReadAsStringAsync();
                    var responseObject = JsonSerializer.Deserialize<List<ApiItem>>(content);

                    if (responseObject != null) {
                        var days = ScheduleFormatter.FormatPeriod(responseObject, groupName);
                        if (days[0].date == DateOnly.FromDateTime(DateTime.Now) &&
                            days[0].items[days[0].items.Count - 1].endTime < TimeOnly.FromDateTime(DateTime.Now)) {
                            return days[1];
                        }
                        return days[0];
                    }
                }
            }
            return null;
        }
        catch {
            throw new Exception("Could not get actual schedule");
        }
    }
}