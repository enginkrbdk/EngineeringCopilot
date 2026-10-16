using OpenAI;
using OpenAI.Chat;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using EngineeringCopilot.Models;

namespace EngineeringCopilot.Services;

public class AiAnalysisService
{
    private readonly ChatClient? _chatClient;

    public AiAnalysisService(IConfiguration configuration)
    {
        var apiKey = configuration["OpenAI:ApiKey"] ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            var model = configuration["OpenAI:Model"] ?? "gpt-4o-mini";
            _chatClient = new ChatClient(model, apiKey);
        }
    }

    public async Task<LogAnalysisResult> AnalyzeErrorLogAsync(LogAnalysisRequest request)
    {
        if (_chatClient is null)
        {
            return AnalyzeLocally(request);
        }

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

        var rawResponse = chatCompletion.Value.Content.FirstOrDefault()?.Text;
        if (string.IsNullOrWhiteSpace(rawResponse))
        {
            return GetFallbackResult("Model boş yanıt verdi.");
        }

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

    private static LogAnalysisResult AnalyzeLocally(LogAnalysisRequest request)
    {
        var message = request.ErrorMessage;
        var lowerMessage = message.ToLowerInvariant();

        if (lowerMessage.Contains("timeout") || lowerMessage.Contains("timed out"))
        {
            return new LogAnalysisResult
            {
                RootCause = $"{request.ServiceName} için bağımlı bir işlem zaman aşımına uğramış. Bu örnek, API anahtarı olmadan çalışan yerel demo analizidir.",
                SuggestedFix = "Bağımlı servisin erişilebilirliğini, yanıt süresini ve bağlantı havuzu sınırlarını kontrol edin.",
                SeverityLevel = "High",
                RequiresImmediateEscalation = false,
                AnalysisMode = "local-demo"
            };
        }

        return new LogAnalysisResult
        {
            RootCause = $"{request.ServiceName} hata kaydı alındı; yerel demo modu belirli bir kök neden saptayamadı.",
            SuggestedFix = "Zaman damgası, ilgili servis logları ve hata anındaki bağımlılık durumunu inceleyin. Gerçek AI analizi için OPENAI_API_KEY tanımlayın.",
            SeverityLevel = "Medium",
            RequiresImmediateEscalation = false,
            AnalysisMode = "local-demo"
        };
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
