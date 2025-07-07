using AINewsEngine.Data;
using AINewsEngine.Models;
using AINewsEngine.Service;
using Microsoft.EntityFrameworkCore;
using System.ServiceModel.Syndication;
using System.Text.RegularExpressions;
using System.Xml;

namespace AINewsEngine.Services
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

        // DEĞİŞİKLİK: Metod imzası, int kategoriId parametresini alacak şekilde güncellendi.
        public async Task<List<Haber>> CekVeKaydetAsync(string rssUrl, int kategoriId)
        {
            var yeniEklenenHaberler = new List<Haber>();
            int islenenHaberSayisi = 0;
            const int haberLimiti = 5;

            try
            {
                var httpClient = _httpClientFactory.CreateClient("RssClient");
                var stream = await httpClient.GetStreamAsync(rssUrl);
                using var xmlReader = XmlReader.Create(stream);
                var feed = SyndicationFeed.Load(xmlReader);

                foreach (var item in feed.Items)
                {
                    if (islenenHaberSayisi >= haberLimiti)
                    {
                        Console.WriteLine($"--> SINIR: {haberLimiti} yeni haber işlendi, işlem durduruluyor.");
                        break;
                    }

                    var orijinalBaslik = item.Title?.Text;
                    if (string.IsNullOrWhiteSpace(orijinalBaslik)) continue;

                    bool haberMevcut = await _context.Haberler.AnyAsync(h => h.Baslik == orijinalBaslik);

                    if (!haberMevcut)
                    {
                        Console.WriteLine("--> SONUÇ: YENİ HABER. LLM işlemi başlatılıyor.");
                        var orijinalIcerik = item.Summary?.Text ?? (item.Content as TextSyndicationContent)?.Text ?? "";

                        orijinalIcerik = Regex.Replace(orijinalIcerik, "<img[^>]*>", string.Empty, RegexOptions.IgnoreCase);

                        var (yeniBaslik, yeniIcerik) = await _llmService.HaberiYenidenYaz(orijinalBaslik, orijinalIcerik);

                        var yeniHaber = new Haber
                        {
                            Baslik = yeniBaslik ?? orijinalBaslik,
                            Icerik = yeniIcerik,
                            YayinTarihi = item.PublishDate.DateTime,
                            ResimUrl = null, // Resim URL'si alınmıyor
                            Onaylandi = true,
                            // DEĞİŞİKLİK: Gelen kategori ID'sini yeni habere atıyoruz.
                            KategoriId = kategoriId
                        };
                        yeniEklenenHaberler.Add(yeniHaber);

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
