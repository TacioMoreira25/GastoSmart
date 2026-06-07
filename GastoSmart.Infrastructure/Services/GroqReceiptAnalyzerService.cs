using System.Text.Json;
using System.Text;
using GastoSmart.Application.DTOs;
using GastoSmart.Application.Services;
using Microsoft.Extensions.Configuration;

namespace GastoSmart.Infrastructure.Services;

public class GroqReceiptAnalyzerService : IReceiptAnalyzerService
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public GroqReceiptAnalyzerService(IConfiguration configuration, HttpClient httpClient)
    {
        _configuration = configuration;
        _httpClient = httpClient;
    }

    public async Task<TransactionRequestDTO> AnalyzeReceiptAsync(Stream imageStream)
    {
        using var memoryStream = new MemoryStream();
        await imageStream.CopyToAsync(memoryStream);
        var base64Image = Convert.ToBase64String(memoryStream.ToArray());
        var imageUrl = $"data:image/jpeg;base64,{base64Image}";

        var apiKey = _configuration["Groq:ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new InvalidOperationException("Groq:ApiKey is not configured.");
        }

        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

        var systemPrompt = @"Você é um assistente financeiro especialista em extrair dados de recibos.
        Vou enviar-lhe a imagem de um recibo. A sua tarefa é analisá-lo e retornar EXCLUSIVAMENTE um JSON.
        O JSON deve ter EXATAMENTE a seguinte estrutura:
        {
          ""Title"": ""Nome do estabelecimento ou resumo claro da compra"",
          ""Amount"": 123.45,
          ""Date"": ""2023-10-25T00:00:00Z""
        }
        Regras:
        - Não escreva NADA além do JSON puro. Sem saudações ou markdown.
        - O 'Amount' DEVE ser o valor TOTAL da fatura, usando ponto para as casas decimais (ex: 15.50).
        - A 'Date' deve ser a data da compra em formato ISO 8601. Se não for visível, use a data de hoje.";

        var payload = new
        {
            model = "meta-llama/llama-4-scout-17b-16e-instruct",            
            messages = new object[]
            {
                new { role = "system", content = systemPrompt },
                new 
                { 
                    role = "user", 
                    content = new object[] 
                    {
                        new { type = "text", text = "Extraia os dados deste recibo de acordo com o esquema JSON solicitado." },
                        new { type = "image_url", image_url = new { url = imageUrl } }
                    }
                }
            },
            temperature = 0.0 
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("https://api.groq.com/openai/v1/chat/completions", content);
        var responseString = await response.Content.ReadAsStringAsync();
        
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Groq API falhou. Status: {response.StatusCode}. Erro: {responseString}");
        }

        using var jsonDocument = JsonDocument.Parse(responseString);
        var messageContent = jsonDocument.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        if (messageContent != null)
        {
            messageContent = messageContent.Trim();
            if (messageContent.StartsWith("```json")) messageContent = messageContent.Substring(7);
            if (messageContent.StartsWith("```")) messageContent = messageContent.Substring(3);
            if (messageContent.EndsWith("```")) messageContent = messageContent.Substring(0, messageContent.Length - 3);
            messageContent = messageContent.Trim();
            
            System.Diagnostics.Debug.WriteLine($"[IA RESPONSE]: {messageContent}");
        }

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var transactionDto = JsonSerializer.Deserialize<TransactionRequestDTO>(messageContent ?? "{}", options);

        if (transactionDto == null) throw new Exception("Falha ao desserializar a resposta da Groq.");

        transactionDto.IsAiGenerated = true;

        return transactionDto;
    }
}