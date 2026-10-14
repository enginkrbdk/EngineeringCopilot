using OpenAI;
using OpenAI.Chat;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using EngineeringCopilot.Models;

namespace EngineeringCopilot.Services;

public class AiAnalysisService
{
    private readonly ChatClient _chatClient;

    public AiAnalysisService(IConfiguration configuration)
    {
        // appsettings.json veya Environment Variable'dan API key çekilir
        var apiKey = configuration["OpenAI:ApiKey"] ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY");

        // Hız ve maliyet dengesi için gpt-4o-mini kullanıyoruz
        _chatClient = new ChatClient("gpt-4o-mini", apiKey);
    }

    public async Task<LogAnalysisResult> AnalyzeErrorLogAsync(LogAnalysisRequest request)
    {
        var systemPrompt = "Sen kıdemli bir DevOps ve Backend mimarısın. Sana iletilen servis hata logunu analiz edeceksin.\n" +
                           "Yanıtını kesinlikle ve sadece aşağıdaki JSON formatında ver, başka hiçbir açıklama metni ekleme:\n" +
                           "{\n  \"rootCause\": \"Hatanın teknik kök nedeni\",\n  \"suggestedFix\": \"Çözüm için atılması gereken adım\",\n  \"severityLevel\": \"Low/Medium/High/Critical\",\n  \"requiresImmediateEscalation\": true veya false\n}";

        var userPrompt = $"Servis Adı: {request.ServiceName}\nHata Mesajı: {request.ErrorMessage}";

        var options = new ChatCompletionOptions
        {
            Temperature = 0.1f // Daha deterministik ve tutarlı yanıtlar için düşük tutuyoruz
        };

        var chatCompletion = await _chatClient.CompleteChatAsync(
            new ChatMessage[]
            {
                new SystemChatMessage(systemPrompt),
                new UserChatMessage(userPrompt)
            }, 
            options
        );

        var rawResponse = chatCompletion.Value.Content[0].Text;

        // Markdown blok temizliği (LLM bazen ```json ... ``` dönebilir)
        var cleanedJson = rawResponse.Replace("```json", "").Replace("```", "").Trim();

        var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        try
        {
            return JsonSerializer.Deserialize<LogAnalysisResult>(cleanedJson, jsonOptions) 
                   ?? GetFallbackResult("JSON parse edilemedi.");
        }
        catch (JsonException)
        {
            // LLM format dışı bir şey döndüğünde sistemin patlamasını önleyen "Graceful Fallback"
            return GetFallbackResult("Model çıktısı beklenen şemaya uymadı.");
        }
    }

    private LogAnalysisResult GetFallbackResult(string reason)
    {
        return new LogAnalysisResult
        {
            RootCause = $"Analiz yapılamadı: {reason}",
            SuggestedFix = "Lütfen ham logları manuel inceleyin.",
            SeverityLevel = "Medium",
            RequiresImmediateEscalation = false
        };
    }
}
