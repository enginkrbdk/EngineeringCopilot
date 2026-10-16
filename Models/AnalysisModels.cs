namespace EngineeringCopilot.Models;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Request model for log analysis.
/// </summary>
public class LogAnalysisRequest
{
    /// <summary>
    /// Hata mesajı (zorunlu).
    /// </summary>
    [Required]
    [MinLength(5)]
    public string ErrorMessage { get; set; } = string.Empty;

    /// <summary>
    /// Hataya ait servis adı.
    /// </summary>
    [Required]
    [StringLength(100)]
    public string ServiceName { get; set; } = string.Empty;
}

/// <summary>
/// AI tarafından döndürülecek analiz sonucu.
/// </summary>
public class LogAnalysisResult
{
    /// <summary>
    /// Kök nedenin kısa açıklaması.
    /// </summary>
    public string RootCause { get; set; } = string.Empty;

    /// <summary>
    /// Önerilen düzeltici eylem.
    /// </summary>
    public string SuggestedFix { get; set; } = string.Empty;

    /// <summary>
    /// Ciddiyet seviyesi: Low, Medium, High, Critical
    /// </summary>
    public string SeverityLevel { get; set; } = "Medium";

    /// <summary>
    /// Hemen eskalasyon gerektirip gerektirmediği.
    /// </summary>
    public bool RequiresImmediateEscalation { get; set; }

    /// <summary>
    /// Analysis source: OpenAI or local-demo.
    /// </summary>
    public string AnalysisMode { get; set; } = "openai";
}
