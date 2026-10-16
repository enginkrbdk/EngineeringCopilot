using EngineeringCopilot.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi(); // .NET'in yerel OpenAPI/Scalar desteği
builder.Services.AddControllers();
builder.Services.AddSingleton<AiAnalysisService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
// .NET'in yerel OpenAPI dokümanını haritalandırır (Varsayılan URL genelde /openapi/v1.json veya /openapi/v1.openapi.json olur)
app.MapOpenApi();

app.UseHttpsRedirection();

app.MapControllers();

app.MapGet("/", () => Results.Content("""
    <!doctype html>
    <html lang="tr">
      <head><meta charset="utf-8"><meta name="viewport" content="width=device-width, initial-scale=1"><title>EngineeringCopilot API</title></head>
      <body style="font-family:system-ui;max-width:760px;margin:3rem auto;padding:0 1rem">
        <h1>EngineeringCopilot API</h1>
        <p>API çalışıyor. Log analizi için <code>POST /api/LogAnalysis/analyze</code> adresini kullanın.</p>
        <p><a href="/openapi/v1.json">OpenAPI şeması</a> · <a href="/routes">Kayıtlı endpoint'ler</a></p>
        <p>Örnek istek için proje içindeki <code>EngineeringCopilot.http</code> dosyasını kullanabilirsiniz.</p>
      </body>
    </html>
    """, "text/html; charset=utf-8"));

// Diagnostic endpoint: Kayıtlı rota/endpoint'leri listeler (404 hatalarını ayıklamak için)
app.MapGet("/routes", (Microsoft.AspNetCore.Routing.EndpointDataSource endpointSource) =>
{
    var list = endpointSource.Endpoints.Select(e => new
    {
        DisplayName = e.DisplayName,
        Endpoint = e.ToString()
    });
    return Results.Ok(list);
});

app.Run();
