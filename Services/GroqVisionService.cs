using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GastoSmart.Services;

public class GroqVisionService : IGroqVisionService
{
    private readonly HttpClient _httpClient;
    private const string GroqApiUrl = "https://api.groq.com/openai/v1/chat/completions";

    public GroqVisionService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> AnalyzeReceiptAsync(string base64Image)
    {
        var apiKey = Preferences.Get("GroqApiKey", string.Empty);
        
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new Exception("Groq API Key not found in Preferences. Please configure it.");
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, GroqApiUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        var payload = new
        {
            model = "llama-3.2-11b-vision-preview", // Or another vision-capable model on Groq
            response_format = new { type = "json_object" },
            messages = new object[]
            {
                new
                {
                    role = "system",
                    content = "Você é um assistente financeiro. Extraia os dados do recibo/nota fiscal fornecido e retorne ESTRITAMENTE um objeto JSON contendo: 'descricao' (string), 'valor' (decimal/number), 'categoria' (string - escolha uma: Alimentação, Transporte, Saúde, Educação, Lazer, Outros) e 'data' (string no formato ISO-8601). Não adicione nenhum outro texto."
                },
                new
                {
                    role = "user",
                    content = new object[]
                    {
                        new { type = "text", text = "Extraia os dados deste recibo." },
                        new { type = "image_url", image_url = new { url = $"data:image/jpeg;base64,{base64Image}" } }
                    }
                }
            },
            temperature = 0.0
        };

        var jsonPayload = JsonSerializer.Serialize(payload);
        request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(jsonResponse);
        
        // Extract the content from the API response
        var content = doc.RootElement
                         .GetProperty("choices")[0]
                         .GetProperty("message")
                         .GetProperty("content")
                         .GetString();

        return content ?? "{}";
    }
}
