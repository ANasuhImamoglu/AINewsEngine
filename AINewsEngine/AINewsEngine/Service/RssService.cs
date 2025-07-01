namespace AINewsEngine.Service;
using AINewsEngine.Data;
using AINewsEngine.Models;
using Microsoft.EntityFrameworkCore;
using System.ServiceModel.Syndication;
using System.Xml;

public class RssService : IRssService
{
    private readonly VeritabaniContext _context;
    private readonly IHttpClientFactory _httpClientFactory;

    public RssService(VeritabaniContext context, IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<List<Haber>> CekVeKaydetAsync(string rssUrl)
    {
        // === DEBUG ADIM 1: Metodun başladığını görelim ===
        Console.WriteLine($"--- RSS Çekme İşlemi Başladı: {rssUrl} ---");

        var yeniEklenenHaberler = new List<Haber>();
        var httpClient = _httpClientFactory.CreateClient("RssClient");

        try
        {
            var stream = await httpClient.GetStreamAsync(rssUrl);
            using var xmlReader = XmlReader.Create(stream);
            var feed = SyndicationFeed.Load(xmlReader);

            // === DEBUG ADIM 2: RSS kaynağından kaç haber geldiğini görelim (EN ÖNEMLİ ADIM) ===
            Console.WriteLine($"RSS kaynağından {feed.Items.Count()} adet öğe bulundu.");

            if (!feed.Items.Any())
            {
                Console.WriteLine("UYARI: RSS kaynağı boş veya öğe bulunamadı. İşlem sonlandırılıyor.");
                return yeniEklenenHaberler; // Boş listeyi hemen döndür.
            }

            foreach (var item in feed.Items)
            {
                var haberBasligi = item.Title?.Text;

                // === DEBUG ADIM 3: Her bir haberin başlığını görelim ===
                Console.WriteLine($"İşlenen başlık: {haberBasligi}");

                if (string.IsNullOrWhiteSpace(haberBasligi))
                {
                    Console.WriteLine("UYARI: Başlığı olmayan bir öğe atlandı.");
                    continue; // Başlığı yoksa bu öğeyi atla ve döngüye devam et.
                }

                bool haberMevcut = await _context.Haberler.AnyAsync(h => h.Baslik == haberBasligi);

                if (!haberMevcut)
                {
                    // === DEBUG ADIM 4: Sadece yeni bir haber eklenirken bunu görelim ===
                    Console.WriteLine($"--> YENİ HABER EKLENİYOR: {haberBasligi}");

                    var yeniHaber = new Haber
                    {
                        Baslik = haberBasligi,
                        Icerik = item.Summary?.Text ?? (item.Content as TextSyndicationContent)?.Text,
                        YayinTarihi = item.PublishDate.DateTime,
                        ResimUrl = item.Links.FirstOrDefault(l => l.MediaType?.StartsWith("image/") ?? false)?.Uri.ToString(),
                        Onaylandi = true
                    };
                    yeniEklenenHaberler.Add(yeniHaber);
                }
            }

            // === DEBUG ADIM 5: Kaç haberin veritabanına kaydedileceğini görelim ===
            Console.WriteLine($"{yeniEklenenHaberler.Count} adet haber veritabanına kaydedilmek üzere hazır.");

            if (yeniEklenenHaberler.Any())
            {
                await _context.Haberler.AddRangeAsync(yeniEklenenHaberler);
                await _context.SaveChangesAsync();
                Console.WriteLine("Başarılı: Yeni haberler veritabanına kaydedildi.");
            }

            return yeniEklenenHaberler;
        }
        catch (Exception ex)
        {
            // === DEBUG ADIM 6: Bir hata olursa detayını görelim ===
            Console.WriteLine($"!!! RSS OKUMA HATASI: {ex.Message}");
            Console.WriteLine(ex.StackTrace); // Hatanın tam kaynağını görmek için
            return new List<Haber>();
        }
    }
}
