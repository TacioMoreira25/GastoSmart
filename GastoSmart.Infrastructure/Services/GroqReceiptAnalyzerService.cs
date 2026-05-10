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
        // 1. OCR with Tesseract
        using var memoryStream = new MemoryStream();
        await imageStream.CopyToAsync(memoryStream);
        var imageBytes = memoryStream.ToArray();

        string extractedText = string.Empty;

        var tempImageFile = Path.GetTempFileName();
        var tempOutFileBase = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        
        try
        {
            await File.WriteAllBytesAsync(tempImageFile, imageBytes);
            
            var processInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "tesseract",
                Arguments = $"\"{tempImageFile}\" \"{tempOutFileBase}\" -l por+eng",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = System.Diagnostics.Process.Start(processInfo);
            if (process != null)
            {
                await process.WaitForExitAsync();
                var outTxtFile = tempOutFileBase + ".txt";
                if (File.Exists(outTxtFile))
                {
                    extractedText = await File.ReadAllTextAsync(outTxtFile);
                }
                else
                {
                    // Fallback without language flag if por+eng isn't installed
                    processInfo.Arguments = $"\"{tempImageFile}\" \"{tempOutFileBase}\"";
                    using var fallbackProcess = System.Diagnostics.Process.Start(processInfo);
                    if (fallbackProcess != null)
                    {
                        await fallbackProcess.WaitForExitAsync();
                        if (File.Exists(outTxtFile))
                        {
                            extractedText = await File.ReadAllTextAsync(outTxtFile);
                        }
                    }
                }
            }
        }
        finally
        {
            if (File.Exists(tempImageFile)) File.Delete(tempImageFile);
            if (File.Exists(tempOutFileBase + ".txt")) File.Delete(tempOutFileBase + ".txt");
        }

        // 2. IA with Groq
        var apiKey = _configuration["Groq:ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new InvalidOperationException("Groq:ApiKey is not configured.");
        }

        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

        var systemPrompt = @"Você é um assistente especializado em extrair dados financeiros de recibos.
        Vou te passar o texto extraído de um recibo por OCR (pode conter erros ou estar sujo).
        Sua tarefa é analisar o texto e retornar EXCLUSIVAMENTE um JSON que represente a transação.
        O JSON deve ter EXATAMENTE a seguinte estrutura:
        {
          ""Title"": ""Nome do estabelecimento ou resumo da compra"",
          ""Amount"": 123.45,
          ""Date"": ""2023-10-25T00:00:00Z""
        }
        Regras:
        - Não inclua nenhuma outra chave.
        - O Amount deve ser um número decimal (use ponto, não vírgula).
        - A Date deve estar em formato ISO 8601.
        - Retorne APENAS o JSON, sem markdown ou texto extra. Isso é crítico.";

        var modelId = _configuration["Groq:ModelId"] ?? "llama-3.1-8b-instant";
        if (modelId == "llama3-8b-8192") modelId = "llama-3.1-8b-instant"; 

        var payload = new
        {
            model = modelId,
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = $"Texto do recibo:\n\n{extractedText}" }
            },
            temperature = 0.0
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("https://api.groq.com/openai/v1/chat/completions", content);
        var responseString = await response.Content.ReadAsStringAsync();
        
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Groq API falhou com status {response.StatusCode} e corpo: {responseString}");
        }

        using var jsonDocument = JsonDocument.Parse(responseString);
        var root = jsonDocument.RootElement;
        
        var messageContent = root.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();

        if (messageContent != null)
        {
            messageContent = messageContent.Trim();
            if (messageContent.StartsWith("```json"))
            {
                messageContent = messageContent.Substring(7);
            }
            if (messageContent.StartsWith("```"))
            {
                messageContent = messageContent.Substring(3);
            }
            if (messageContent.EndsWith("```"))
            {
                messageContent = messageContent.Substring(0, messageContent.Length - 3);
            }
            messageContent = messageContent.Trim();
        }

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var transactionDto = JsonSerializer.Deserialize<TransactionRequestDTO>(messageContent ?? "{}", options);

        if (transactionDto == null)
        {
            throw new Exception("Falha ao desserializar a resposta da Groq.");
        }

        transactionDto.IsAiGenerated = true;

        return transactionDto;
    }
}
