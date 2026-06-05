using System.Net.Http.Headers;
using System.Net.Http.Json;
using Gastosmart.App.DTOs;

namespace GastoSmart.App.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;

    public ApiService(HttpClient httpClient )
    {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(90);
    }

    public async Task<TransactionRequestDTO?> ScanReceiptAsync(FileResult photo)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[ApiService] Starting ScanReceiptAsync for file: {photo.FileName}");
            
            // Abre a foto de forma segura
            using var stream = await photo.OpenReadAsync();
            using var streamContent = new StreamContent(stream);
            
            // Define que é uma imagem
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(photo.ContentType ?? "image/jpeg");

            using var formData = new MultipartFormDataContent();
            formData.Add(streamContent, "receiptImage", photo.FileName);
            
            // Dispara para a API
            var response = await _httpClient.PostAsync("/api/Transactions/scan-receipt", formData);

            System.Diagnostics.Debug.WriteLine($"[ApiService] Response status: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<TransactionRequestDTO>();
                System.Diagnostics.Debug.WriteLine($"[ApiService] Successfully parsed response");
                return result;
            }
            else
            {
                throw new Exception($"A API recusou a foto. Status: {response.StatusCode}");
            }
        }
        catch (HttpRequestException hre)
        {
            var errorMessage = $"Falha de conexão com a API em {_httpClient.BaseAddress}. Se estiver em emulador use 10.0.2.2, se estiver em celular físico use o IP da sua máquina na rede local. Erro: {hre.Message}";
            System.Diagnostics.Debug.WriteLine($"[ApiService] HttpRequestException: {errorMessage}");
            throw new Exception(errorMessage, hre);
        }
        catch (TaskCanceledException tce)
        {
            var errorMessage = $"Timeout ao conectar com a API ({_httpClient.Timeout.TotalSeconds}s). A API pode estar lenta ou inacessível. URL atual: {_httpClient.BaseAddress}. Erro: {tce.Message}";
            System.Diagnostics.Debug.WriteLine($"[ApiService] Timeout: {errorMessage}");
            throw new Exception(errorMessage, tce);
        }
        catch (Exception ex)
        {
            var errorMessage = $"Falha ao enviar foto: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"[ApiService] General Exception: {errorMessage}");
            throw new Exception(errorMessage, ex);
        }
    }
}

