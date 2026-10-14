# EngineeringCopilot

EngineeringCopilot — .NET 10 tabanlı örnek WebAPI proje. AI destekli log analizi servisi içerir.

Yerel olarak çalıştırmak için:

1. .NET 10 SDK yüklü olmalı.
2. Terminalde proje dizinine gidin:
   cd C:\Projeler\EngineeringCopilot\
3. Uygulamayı çalıştırın:
   dotnet run

API endpointleri:
- POST /api/LogAnalysis/analyze — log analizi

Not: OpenAI API anahtarınızı yapılandırın (appsettings.json veya OPENAI_API_KEY environment variable).