using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Maui.Storage;

namespace GastoSmart.App.Services;

public class AuthService
{
    private readonly HttpClient _httpClient;

    public AuthService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> LoginAsync(string email, string password)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, LocalConfig.SupabaseAuthUrl);
        request.Headers.Add("apikey", LocalConfig.SupabaseAnonKey);

        var payload = new { email, password };
        request.Content = JsonContent.Create(payload);

        var response = await _httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<SupabaseAuthResponse>();
            if (result?.AccessToken != null)
            {
                await SecureStorage.Default.SetAsync("jwt_token", result.AccessToken);
                
                if (result.User?.Id != null)
                {
                    await SecureStorage.Default.SetAsync("user_id", result.User.Id);
                }
                
                return true;
            }
        }
        return false;
    }

    public void Logout()
    {
        SecureStorage.Default.Remove("jwt_token");
        SecureStorage.Default.Remove("user_id");
    }

    private class SupabaseAuthResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }
        
        [JsonPropertyName("user")]
        public SupabaseUser? User { get; set; }
    }

    private class SupabaseUser
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }
    }
}