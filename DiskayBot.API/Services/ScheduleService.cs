using DiskayBot.API.Contracts.Schedule;
using DiskayBot.API.Contracts.Service;
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

    public async Task<DayScheduleResponse?> GetActualSchedule(string groupName) {
        try {
            var response =
                await _client.GetAsync($"{_baseUrl}/api/DaySchedule/GetActualSchedule?group_name={groupName}");

            if (response.IsSuccessStatusCode) {
                var content = await response.Content.ReadAsStringAsync();
                var scheduleDay = JsonConvert.DeserializeObject<DayScheduleResponse>(content);

                if (scheduleDay != null) return scheduleDay;
                return null;
            }

            return null;
        }
        catch (Exception ex) {
            return null;
        }
    }

    public async Task<DayScheduleResponse?> GetDaySchedule(DateOnly date, string groupName) {
        try {
            var requestBody = DayScheduleRequest.Create(date, groupName);
            
            Console.WriteLine("Получаю расписание");
            
            var response =
                await _client.GetAsync(
                    $"{_baseUrl}/api/DaySchedule/GetDayByDate?date={requestBody.Date}&group_name={requestBody.GroupName}");

            if (response.IsSuccessStatusCode) {
                Console.WriteLine("Расписание получено");
                var content = await response.Content.ReadAsStringAsync();
                var scheduleDay = JsonConvert.DeserializeObject<DayScheduleResponse>(content);

                if (scheduleDay != null) return scheduleDay;
                return null;
            }
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound) {
                return null;
            }
            throw new Exception("Could not get day schedule");
        }
        
        catch (HttpRequestException ex) {
            throw new Exception("Could not get day schedule", ex);
        }
    }

    public async Task<PingResponse> PingService() {
        try{
            var response = await _client.GetAsync($"{_baseUrl}/api/Service/Ping");

            if (response.IsSuccessStatusCode){
                var content = await response.Content.ReadAsStringAsync();
                var responseObject = JsonSerializer.Deserialize<PingResponse>(content);
                if (responseObject != null){
                    return responseObject;
                }
            }

            throw new Exception(response.ReasonPhrase);
        }
        catch (HttpRequestException){
            return PingResponse.CreateDefault(Name);
        }

        catch (Exception ex){
            throw new Exception(ex.Message);
        }
    }
}