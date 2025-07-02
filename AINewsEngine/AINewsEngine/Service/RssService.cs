using System.ServiceModel.Syndication;
using System.Text.RegularExpressions;
using System.Xml;
using Microsoft.EntityFrameworkCore;
using AINewsEngine.Models;
using AINewsEngine.Data;

namespace AINewsEngine.Service
{
    public class RssService : IRssService
    {
        private readonly VeritabaniContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILlmService _llmService;

        public RssService(VeritabaniContext context, IHttpClientFactory httpClientFactory, ILlmService llmService)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _llmService = llmService;
        }

        public async Task<List<Haber>> CekVeKaydetAsync(string rssUrl)
        {
            var yeniEklenenHaberler = new List<Haber>();

            // YENİ: Yapay zeka tarafından işlenen haber sayısını tutmak için bir sayaç
            int islenenHaberSayisi = 0;
            const int haberLimiti = 5; // Limiti buradan kolayca değiştirebilirsiniz

            try
            {
                var httpClient = _httpClientFactory.CreateClient("RssClient");
                var stream = await httpClient.GetStreamAsync(rssUrl);
                using var xmlReader = XmlReader.Create(stream);
                var feed = SyndicationFeed.Load(xmlReader);

                foreach (var item in feed.Items)
                {
                    // DEĞİŞİKLİK: Eğer 5 yeni haber işlediysek, döngüyü tamamen durdur.
                    if (islenenHaberSayisi >= haberLimiti)
                    {
                        Console.WriteLine($"--> SINIR: {haberLimiti} yeni haber işlendi, işlem durduruluyor.");
                        break; // foreach döngüsünden çık
                    }

                    var orijinalBaslik = item.Title?.Text;
                    if (string.IsNullOrWhiteSpace(orijinalBaslik)) continue;

                    bool haberMevcut = await _context.Haberler.AnyAsync(h => h.Baslik == orijinalBaslik);

                    if (!haberMevcut)
                    {
                        Console.WriteLine("--> SONUÇ: YENİ HABER. LLM işlemi başlatılıyor.");
                        var orijinalIcerik = item.Summary?.Text ?? (item.Content as TextSyndicationContent)?.Text ?? "";

                        // Bu kod, içerik metnindeki <img ... > gibi tüm etiketleri bulur
                        // ve onları boşlukla değiştirerek metinden tamamen temizler.
                        orijinalIcerik = Regex.Replace(orijinalIcerik, "<img[^>]*>", string.Empty, RegexOptions.IgnoreCase);

                        var (yeniBaslik, yeniIcerik) = await _llmService.HaberiYenidenYaz(orijinalBaslik, orijinalIcerik);

                        var yeniHaber = new Haber
                        {
                            Baslik = yeniBaslik ?? orijinalBaslik,
                            Icerik = yeniIcerik,
                            YayinTarihi = item.PublishDate.DateTime,
                            ResimUrl = item.Links.FirstOrDefault(l => l.MediaType?.StartsWith("image/") ?? false)?.Uri.ToString(),
                            Onaylandi = true
                        };
                        yeniEklenenHaberler.Add(yeniHaber);

                        // DEĞİŞİKLİK: Sadece bir haber başarıyla işlendiğinde sayacı artır.
                        islenenHaberSayisi++;
                    }
                }

                if (yeniEklenenHaberler.Any())
                {
                    await _context.Haberler.AddRangeAsync(yeniEklenenHaberler);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"!!! RSS OKUMA HATASI: {ex.Message}");
                yeniEklenenHaberler.Clear();
            }

            return yeniEklenenHaberler;
        }
    }
}
