using AINewsEngine.Models;

namespace AINewsEngine.Service
{
    public interface IRssService
    {
        // Verilen URL'den haberleri çeker, veritabanına kaydeder
        // ve yeni eklenen haberlerin listesini döndürür.
        Task<List<Haber>> CekVeKaydetAsync(string rssUrl);
    }
}
