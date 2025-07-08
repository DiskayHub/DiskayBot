using System;
using System.Net;
using System.Text;
using System.Text.Json;

namespace DiskayBot.API.Services;

public class BotService{
    private static readonly HttpClient _client = new();

    public static async Task<HttpStatusCode> registration(long userId, string userName, string groupId) {
        try {
            string json_content = JsonSerializer.Serialize(new {
                user_id = userId,
                username = userName,
                group_id = groupId
            });
            var content = new StringContent(json_content, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _client.PostAsync("http://localhost:5014/api/TelegramUser/AddUser", content);
            string ResponseBody = await response.Content.ReadAsStringAsync();

            return HttpStatusCode.OK;
        }

        catch (HttpRequestException ex) {
            Console.WriteLine("Ошибка дурацкая: " + ex.Message);
            return HttpStatusCode.BadRequest;
        }
    }

    public static async Task<HttpStatusCode> authorization(long userId) {
        try {
            string json_content = JsonSerializer.Serialize(new {
                user_id = userId
            });
            var content = new StringContent(json_content, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _client.PostAsync("http://localhost:5014/api/TelegramUser/GetById", content);
            return response.StatusCode;
        }
        catch (HttpRequestException ex) {
            Console.WriteLine("Ошибка дурацкая: " + ex.Message);
            return HttpStatusCode.BadRequest;
        }
    }
}
