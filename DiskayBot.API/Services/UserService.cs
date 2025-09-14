using System;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using DiskayBot.API.Contracts;
using DiskayBot.API.Contracts.Groups;
using DiskayBot.API.Contracts.Service;
using DiskayBot.API.Contracts.Users.UpdateUser;

namespace DiskayBot.API.Services;

public class UserService{
    private readonly HttpClient _client;
    private readonly string _baseUrl;
    public string Name { get; }

    public UserService(HttpClient client,  string baseUrl, string name) {
        _client = client;
        _baseUrl = baseUrl;
        Name = name;
    }

    public async Task<PingResponse?> PingService() {
        try{
            var response = await _client.GetAsync($"{_baseUrl}/api/service/ping");

            if (response.IsSuccessStatusCode){
                var content = await response.Content.ReadAsStringAsync();
                var responseStatus = JsonSerializer.Deserialize<PingResponse>(content);
                return responseStatus;
            }

            return null;
        }
        catch (HttpRequestException){
            return PingResponse.CreateDefault(Name);
        }
        catch (Exception ex){
            throw new Exception(ex.Message);
        }
    }

    public async Task<HttpStatusCode> Registration(long userId, string userName, string groupId) {
        try {
            string jsonContent = JsonSerializer.Serialize(new {
                user_id = userId,
                username = userName,
                group_id = groupId
            });
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync($"{_baseUrl}/api/diskay/telegram_user", content);
            
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
            string jsonContent = JsonSerializer.Serialize(new {
                user_id = userId
            });
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            HttpResponseMessage response =
                await _client.GetAsync($"{_baseUrl}/api/telegram_users/{userId}");

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
            HttpResponseMessage response = await _client.GetAsync($"{_baseUrl}/api/groups");
            if (response.IsSuccessStatusCode) {
                string responseBody = await response.Content.ReadAsStringAsync();
                var groups = JsonSerializer.Deserialize<List<GroupResponse>>(responseBody);
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
                await _client.GetAsync($"{_baseUrl}/api/groups/{course}");
            if (response.IsSuccessStatusCode){
                string responseBody = await response.Content.ReadAsStringAsync();
                var groups = JsonSerializer.Deserialize<List<GroupResponse>>(responseBody);
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

    public async Task<HttpStatusCode> UpdateUser(long userId, UpdateUserRequest requestBody) {
        try {
            var jsonString = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            var response = await _client.PutAsync($"{_baseUrl}/api/telegram_users/{userId}", content);

            if (response.IsSuccessStatusCode) {
                return HttpStatusCode.OK;
            }
            return HttpStatusCode.InternalServerError;
        }
        catch (HttpRequestException ex) {
            throw new HttpRequestException(ex.Message);
        }
    }
}
