using System.Net.Http.Headers;
using System.Net.Http.Json;
using Gastosmart.App.DTOs;
namespace GastoSmart.App.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly ISafeStorageService _safeStorage;

    public ApiService(HttpClient httpClient, ISafeStorageService safeStorage)
    {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(90);
        _safeStorage = safeStorage;
    }
    
    private async Task AddAuthorizationHeaderAsync(HttpRequestMessage request)
    {
        var token = await _safeStorage.GetTokenAsync("jwt_token");
        if (string.IsNullOrEmpty(token))
        {
            throw new UnauthorizedAccessException("Token de autenticação não encontrado. Por favor, faça login novamente.");
        }
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }
    
    public async Task<TransactionRequestDTO?> ScanReceiptAsync(FileResult photo)
    {
        try
        {
            using var stream = await photo.OpenReadAsync();
            using var streamContent = new StreamContent(stream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(photo.ContentType ?? "image/jpeg");

            using var formData = new MultipartFormDataContent();
            formData.Add(streamContent, "receiptImage", photo.FileName);
            
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/api/Transactions/scan-receipt");
            requestMessage.Content = formData;

            await AddAuthorizationHeaderAsync(requestMessage);

            var url = _httpClient.BaseAddress?.ToString().TrimEnd('/') + "/api/Transactions/scan-receipt";
            Console.WriteLine($"[GastoSmart-HttpLog] Tentando conectar em: {url}");
            var response = await _httpClient.SendAsync(requestMessage);

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
            Console.WriteLine($"[GastoSmart-HttpError] EXCEPTION TIPO: {ex.GetType().Name}");
            Console.WriteLine($"[GastoSmart-HttpError] MENSAGEM: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"[GastoSmart-HttpError] INNER MENSAGEM: {ex.InnerException.Message}");
            }
            throw new Exception($"Falha ao enviar foto: {ex.Message}", ex);
        }
    }

    public async Task<bool> SaveTransactionAsync(TransactionRequestDTO transaction)
    {
        var userIdStr = await _safeStorage.GetTokenAsync("user_id");
        if (Guid.TryParse(userIdStr, out var userId))
        {
            transaction.UserId = userId;
        }

        try
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/api/Transactions");
            requestMessage.Headers.Add("X-Idempotency-Key", Guid.NewGuid().ToString());
            requestMessage.Content = JsonContent.Create(transaction);

            await AddAuthorizationHeaderAsync(requestMessage);

            var url = _httpClient.BaseAddress?.ToString().TrimEnd('/') + "/api/Transactions";
            Console.WriteLine($"[GastoSmart-HttpLog] Tentando conectar em: {url}");
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
            Console.WriteLine($"[GastoSmart-HttpError] EXCEPTION TIPO: {ex.GetType().Name}");
            Console.WriteLine($"[GastoSmart-HttpError] MENSAGEM: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"[GastoSmart-HttpError] INNER MENSAGEM: {ex.InnerException.Message}");
            }
            System.Diagnostics.Debug.WriteLine($"[ApiService] Erro fatal ao salvar: {ex.Message}");
            return false;
        }
    }

    public async Task<DashboardSummaryDTO?> GetDashboardSummaryAsync()
    {
        try
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, "/api/Transactions/dashboard-summary");
            
            await AddAuthorizationHeaderAsync(requestMessage);

            var url = _httpClient.BaseAddress?.ToString().TrimEnd('/') + "/api/Transactions/dashboard-summary";
            Console.WriteLine($"[GastoSmart-HttpLog] Tentando conectar em: {url}");
            var response = await _httpClient.SendAsync(requestMessage);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException("Token expirado ou inválido.");
            }

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<DashboardSummaryDTO>();
            }
            
            var error = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"[ApiService] Erro ao buscar dashboard: {response.StatusCode} - {error}");
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            throw; // Re-throw to handle in the ViewModel (redirect to login)
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GastoSmart-HttpError] EXCEPTION TIPO: {ex.GetType().Name}");
            Console.WriteLine($"[GastoSmart-HttpError] MENSAGEM: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"[GastoSmart-HttpError] INNER MENSAGEM: {ex.InnerException.Message}");
            }
            System.Diagnostics.Debug.WriteLine($"[ApiService] Erro fatal ao buscar dashboard: {ex.Message}");
            return null;
        }
    }

    public async Task<List<GastoSmart.App.ViewModels.Transacao>?> GetTransacoesAsync()
    {
        try
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, "/api/Transactions");
            
            await AddAuthorizationHeaderAsync(requestMessage);

            var url = _httpClient.BaseAddress?.ToString().TrimEnd('/') + "/api/Transactions";
            Console.WriteLine($"[GastoSmart-HttpLog] Tentando conectar em: {url}");
            var response = await _httpClient.SendAsync(requestMessage);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<GastoSmart.App.ViewModels.Transacao>>();
            }
            
            var error = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"[ApiService] Erro ao buscar transacoes: {response.StatusCode} - {error}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GastoSmart-HttpError] EXCEPTION TIPO: {ex.GetType().Name}");
            Console.WriteLine($"[GastoSmart-HttpError] MENSAGEM: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"[GastoSmart-HttpError] INNER MENSAGEM: {ex.InnerException.Message}");
            }
            System.Diagnostics.Debug.WriteLine($"[ApiService] Erro fatal ao buscar transacoes: {ex.Message}");
            return null;
        }
    }
}