using System;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using DiskayBot.API.Contracts;
using DiskayBot.API.Contracts.Groups;

namespace DiskayBot.API.Services;

public class MemoryService{
    private readonly HttpClient _client;
    public string Name { get; }

    public MemoryService(HttpClient client) {
        _client = client;
        Name = "DiskayMemory";
    }

    public async Task<HttpStatusCode> Registration(long userId, string userName, string groupId) {
        try {
            string json_content = JsonSerializer.Serialize(new {
                user_id = userId,
                username = userName,
                group_id = groupId
            });
            var content = new StringContent(json_content, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _client.PostAsync("http://localhost:5014/api/TelegramUsers/AddUser", content);
            
            Console.WriteLine(response.RequestMessage);
            if (response.IsSuccessStatusCode) {
                return HttpStatusCode.OK;   
            }
            return HttpStatusCode.InternalServerError;
        }

        catch (HttpRequestException ex) {
            Console.WriteLine("Ошибка дурацкая: " + ex.Message);
            return HttpStatusCode.BadRequest;
        }
    }

    public async Task<UserData?> Authorization(long userId) {
        try{
            string json_content = JsonSerializer.Serialize(new {
                user_id = userId
            });
            var content = new StringContent(json_content, Encoding.UTF8, "application/json");
            HttpResponseMessage response =
                await _client.GetAsync($"http://localhost:5014/api/TelegramUsers/GetById?user_id={userId}");

            if (response.IsSuccessStatusCode){
                var responseBody = await response.Content.ReadAsStringAsync();
                var userData = JsonSerializer.Deserialize<UserData>(responseBody);
                return userData;
            }

            if (response.StatusCode == HttpStatusCode.NotFound){
                return null;
            }

            throw new HttpRequestException();
        }
        catch (HttpRequestException ex){
            Console.WriteLine("Ошибка дурацкая: " + ex.Message);
            throw new HttpRequestException(ex.Message);
        }
        catch (Exception ex){
            throw new Exception(ex.Message);
        }
    }

    public async Task<List<GroupResponse>?> GetAllGroups() {
        try {
            HttpResponseMessage response = await _client.GetAsync("http://localhost:5014/api/Groups/GetAll");
            if (response.IsSuccessStatusCode) {
                string ResponseBody = await response.Content.ReadAsStringAsync();
                var groups = JsonSerializer.Deserialize<List<GroupResponse>>(ResponseBody);
                return groups;
            }
            throw new HttpRequestException();
        }
        catch (HttpRequestException ex) {
            throw new HttpRequestException(ex.Message);
        }
        catch (Exception ex) {
            Console.WriteLine(ex.GetType());
            Console.WriteLine(ex.Message);
            return null;
        }
    }

    public async Task<List<GroupResponse>?> GetCourseGroups(int course) {
        try{
            HttpResponseMessage response =
                await _client.GetAsync($"http://localhost:5014/api/Groups/GetByCourse?course={course}");
            if (response.IsSuccessStatusCode){
                string ResponseBody = await response.Content.ReadAsStringAsync();
                var groups = JsonSerializer.Deserialize<List<GroupResponse>>(ResponseBody);
                return groups;
            }

            throw new HttpRequestException();
        }
        catch (HttpRequestException ex){
            throw new HttpRequestException(ex.Message);
        }
        catch (Exception ex){
            throw new Exception(ex.Message);
        }
    }
}
