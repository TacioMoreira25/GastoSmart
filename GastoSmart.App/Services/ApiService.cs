using System.Net.Http.Headers;
using System.Net.Http.Json;
using Gastosmart.App.DTOs;
using Microsoft.Maui.Storage; 

namespace GastoSmart.App.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(90);
    }

    private async Task AdicionarTokenDeSegurancaAsync()
    {
        var token = await SecureStorage.Default.GetAsync("jwt_token");
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    public async Task<TransactionRequestDTO?> ScanReceiptAsync(FileResult photo)
    {
        await AdicionarTokenDeSegurancaAsync();

        try
        {
            System.Diagnostics.Debug.WriteLine($"[ApiService] Starting ScanReceiptAsync for file: {photo.FileName}");
            
            using var stream = await photo.OpenReadAsync();
            using var streamContent = new StreamContent(stream);
            
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(photo.ContentType ?? "image/jpeg");

            using var formData = new MultipartFormDataContent();
            formData.Add(streamContent, "receiptImage", photo.FileName);
            
            var response = await _httpClient.PostAsync("/api/Transactions/scan-receipt", formData);

            System.Diagnostics.Debug.WriteLine($"[ApiService] Response status: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<TransactionRequestDTO>();
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"A API recusou a foto. Status: {response.StatusCode} - {error}");
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Falha ao enviar foto: {ex.Message}", ex);
        }
    }

    public async Task<bool> SaveTransactionAsync(TransactionRequestDTO transaction)
    {
        await AdicionarTokenDeSegurancaAsync();
        
        var userIdStr = await SecureStorage.Default.GetAsync("user_id");
        if (Guid.TryParse(userIdStr, out var userId))
        {
            transaction.UserId = userId;
        }

        try
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/api/Transactions");
            requestMessage.Headers.Add("X-Idempotency-Key", Guid.NewGuid().ToString());
            requestMessage.Content = JsonContent.Create(transaction);

            var response = await _httpClient.SendAsync(requestMessage);

            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            
            var error = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"[ApiService] Erro ao salvar: {response.StatusCode} - {error}");
            return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ApiService] Erro fatal ao salvar: {ex.Message}");
            return false;
        }
    }
}