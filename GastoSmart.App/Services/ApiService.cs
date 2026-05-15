using System.Net.Http.Headers;
using System.Text.Json;
using Gastosmart.App.DTOs;

namespace Gastosmart.App.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;

    public ApiService()
    {
        _httpClient = new HttpClient();
        
#if ANDROID
        _httpClient.BaseAddress = new Uri("http://10.0.2.2:5146");
#else
        _httpClient.BaseAddress = new Uri("http://localhost:5146");
#endif
    }

    public async Task<TransactionRequestDTO?> ScanReceiptAsync(FileResult photo)
    {
        using var stream = await photo.OpenReadAsync();
        using var content = new MultipartFormDataContent();
        
        var streamContent = new StreamContent(stream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(photo.ContentType ?? "image/jpeg");
        
        content.Add(streamContent, "receipt", photo.FileName);

        var response = await _httpClient.PostAsync("/api/Transactions/scan-receipt", content);
        
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<TransactionRequestDTO>(json, options);
        }

        return null;
    }
}
