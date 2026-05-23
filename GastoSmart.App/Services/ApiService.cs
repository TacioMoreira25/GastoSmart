using System.Net.Http.Headers;
using System.Net.Http.Json;
using Gastosmart.App.DTOs;

namespace GastoSmart.App.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;

    public ApiService()
    {
        _httpClient = new HttpClient();
        
        _httpClient.BaseAddress = new Uri("http://192.168.1.68:5146"); 
        _httpClient.Timeout = TimeSpan.FromSeconds(120); 
    }

    public async Task<TransactionRequestDTO?> ScanReceiptAsync(FileResult photo)
    {
        try
        {
            // Abre a foto de forma segura
            using var stream = await photo.OpenReadAsync();
            using var streamContent = new StreamContent(stream);
            
            // Define que é uma imagem
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(photo.ContentType);

            using var formData = new MultipartFormDataContent();
            
            formData.Add(streamContent, "receiptImage", photo.FileName);

            // Dispara para a API
            var response = await _httpClient.PostAsync("/api/Transactions/scan-receipt", formData);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<TransactionRequestDTO>();
            }
            else
            {
                var erroApi = await response.Content.ReadAsStringAsync();
                throw new Exception($"A API recusou a foto. Status: {response.StatusCode}. Detalhe: {erroApi}");
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Falha ao enviar foto: {ex.Message}");
        }
    }
}