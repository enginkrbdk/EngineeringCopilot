using Microsoft.AspNetCore.Mvc;
using EngineeringCopilot.Services;
using EngineeringCopilot.Models;

namespace EngineeringCopilot.Controllers;

/// <summary>
/// Log analizine yönelik endpointler.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class LogAnalysisController : ControllerBase
{
    private readonly AiAnalysisService _aiAnalysisService;

    public LogAnalysisController(AiAnalysisService aiAnalysisService)
    {
        _aiAnalysisService = aiAnalysisService;
    }

    /// <summary>
    /// AI ile log analizi yapar ve kök neden / çözüm önerisi döner.
    /// </summary>
    /// <param name="request">Analiz edilecek hata logu ve servis bilgisi.</param>
    /// <returns>LogAnalysisResult içeren 200 yanıtı veya hata bilgisi.</returns>
    [HttpPost("analyze")]
    [ProducesResponseType(typeof(LogAnalysisResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AnalyzeLog([FromBody] LogAnalysisRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var result = await _aiAnalysisService.AnalyzeErrorLogAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            // Kurumsal standart: Detaylı hata dışarı sızdırılmaz, loglanır
            return StatusCode(500, new { error = "AI analizi sırasında bir hata oluştu.", details = ex.Message });
        }
    }
}
