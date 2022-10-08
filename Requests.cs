using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SimpleChatAppLibrary;

public class Requests {

    public static string SendHttpRequest(string requestType, string url, string? jsonData, string? pw = null) {
        HttpClient client = new HttpClient();
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        if (pw != null) {
            client.DefaultRequestHeaders.Add("Authorization", Convert.ToBase64String(Encoding.UTF8.GetBytes(pw)));
        }
        HttpResponseMessage? response = requestType switch {
            "POST" => client.PostAsync(url, new StringContent(jsonData, System.Text.Encoding.UTF8, "application/json"))
                .Result,
            "GET" => client.GetAsync(url).Result,
            _ => throw new ArgumentException("Invalid request type")
        };
        string respText = response.Content.ReadAsStringAsync().Result;
        return respText;
    }

    public static string SendHttpRequest(string requestType, string url, Dictionary<string, object>? jsonData, string? pw = null) 
        => SendHttpRequest(requestType, url, JsonSerializer.Serialize(jsonData), pw);
    
    public static string SendHttpRequest(string requestType, string url, string? pw = null) 
        => SendHttpRequest(requestType, url, string.Empty, pw);

}