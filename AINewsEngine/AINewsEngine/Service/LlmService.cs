using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AINewsEngine.Service
{
    public class YenidenYazilmisHaber
    {
        [JsonPropertyName("yeni_baslik")]
        public string? YeniBaslik { get; set; }

        [JsonPropertyName("yeni_icerik")]
        public string? YeniIcerik { get; set; }
    }

    public class LlmService : ILlmService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private const string OpenRouterApiUrl = "https://openrouter.ai/api/v1/chat/completions";

        public LlmService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<(string? YeniBaslik, string? YeniIcerik)> HaberiYenidenYaz(string orijinalBaslik, string orijinalIcerik)
        {
            var apiKey = _configuration["OpenRouter:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                // === DEBUG 1: API anahtarı var mı? ===
                Console.WriteLine("!!! HATA: OpenRouter API anahtarı user-secrets içinde bulunamadı.");
                return (orijinalBaslik, orijinalIcerik);
            }

            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            httpClient.DefaultRequestHeaders.Add("HTTP-Referer", "http://localhost");
            httpClient.DefaultRequestHeaders.Add("X-Title", "AI News Engine");

            var prompt = $@"Sen profesyonel bir haber editörüsün. Sana verilen bir haber başlığını ve içeriğini, özünü koruyarak, tarafsız bir dille ve daha akıcı bir üslupla yeniden yazmanı istiyorum. Cevabın sadece ve sadece JSON formatında olmalı ve şu yapıda olmalı: {{""yeni_baslik"": ""Yeniden Yazılmış Başlık"", ""yeni_icerik"": ""Yeniden yazılmış haber metni...""}}
---
Orijinal Başlık: {orijinalBaslik}
Orijinal İçerik: {orijinalIcerik}";

            var requestBody = new
            {
                model = "deepseek/deepseek-r1:free",
                messages = new[] { new { role = "system", content = prompt } },
                response_format = new { type = "json_object" }
            };

            var jsonRequestBody = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

            try
            {
                // === DEBUG 2: API'ye istek göndermeden hemen önce loglama ===
                Console.WriteLine("--> OpenRouter API'sine istek gönderiliyor...");
                var response = await httpClient.PostAsync(OpenRouterApiUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    // === DEBUG 3: API'den başarısız bir yanıt gelirse, yanıtın içeriğini yazdır ===
                    var errorBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"!!! OpenRouter API Hatası: {response.StatusCode}");
                    Console.WriteLine($"!!! Hata Detayı: {errorBody}");
                    return (orijinalBaslik, orijinalIcerik);
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(jsonResponse);
                var llmTextOutput = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();

                if (llmTextOutput != null)
                {
                    Console.WriteLine("--> Başarılı: LLM'den yanıt alındı ve parse edildi.");
                    var yenidenYazilmisHaber = JsonSerializer.Deserialize<YenidenYazilmisHaber>(llmTextOutput);
                    return (yenidenYazilmisHaber?.YeniBaslik, yenidenYazilmisHaber?.YeniIcerik);
                }
            }
            catch (Exception ex)
            {
                // === DEBUG 4: İstek sırasında herhangi bir exception olursa onu yazdır ===
                Console.WriteLine($"!!! LLM isteği sırasında genel hata: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }

            Console.WriteLine("--> Uyarı: LLM'den geçerli bir yanıt alınamadı, orijinal metin kullanılıyor.");
            return (orijinalBaslik, orijinalIcerik);
        }
    }
}
