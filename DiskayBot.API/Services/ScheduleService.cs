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