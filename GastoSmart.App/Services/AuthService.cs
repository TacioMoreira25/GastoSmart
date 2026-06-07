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

    public async Task<(bool IsSuccess, string ErrorMessage)> RegisterAsync(string name, string email, string password)
    {
        string signupUrl = LocalConfig.SupabaseAuthUrl.Replace("/token?grant_type=password", "/signup");
        
        var request = new HttpRequestMessage(HttpMethod.Post, signupUrl);
        request.Headers.Add("apikey", LocalConfig.SupabaseAnonKey);

        var payload = new 
        { 
            email, 
            password,
            data = new { name }
        };
        request.Content = JsonContent.Create(payload);

        var response = await _httpClient.SendAsync(request);
        
        if (response.IsSuccessStatusCode)
        {
            return (true, string.Empty);
        }

        var errorContent = await response.Content.ReadAsStringAsync();
        string errorMessage = "Falha ao criar conta. Tente novamente.";
        
        try 
        {
            var errorObj = System.Text.Json.JsonSerializer.Deserialize<SupabaseErrorResponse>(errorContent);
            var parsedMessage = errorObj?.GetErrorMessage();
            
            if (!string.IsNullOrEmpty(parsedMessage))
            {
                if (parsedMessage.Contains("already registered", StringComparison.OrdinalIgnoreCase) ||
                    parsedMessage.Contains("already exists", StringComparison.OrdinalIgnoreCase))
                {
                    errorMessage = "Este e-mail já está cadastrado em nosso sistema.";
                }
                else
                {
                    errorMessage = parsedMessage;
                }
            }
        }
        catch 
        {
            // Fallback para a mensagem genérica se falhar ao desserializar
        }

        return (false, errorMessage);
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

    private class SupabaseErrorResponse
    {
        [JsonPropertyName("msg")]
        public string? Msg { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }
        
        [JsonPropertyName("error_description")]
        public string? ErrorDescription { get; set; }
        
        public string? GetErrorMessage() => Msg ?? Message ?? ErrorDescription;
    }
}