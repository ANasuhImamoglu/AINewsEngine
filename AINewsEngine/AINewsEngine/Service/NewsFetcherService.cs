using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using AINewsEngine.Service; // IRssService namespace'i

namespace AINewsEngine.Service
{
    public class NewsFetcherService : BackgroundService
    {
        private readonly ILogger<NewsFetcherService> _logger;


        public NewsFetcherService(ILogger<NewsFetcherService> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("NewsFetcherService başlatıldı.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Bir sonraki sabah 9'u hesapla
                    var now = DateTime.Now;
                    var targetTimeToday = new DateTime(now.Year, now.Month, now.Day, 15, 36, 0);
                    var nextRun = now < targetTimeToday ? targetTimeToday : targetTimeToday.AddDays(0);
                    var delay = nextRun - now;

                    _logger.LogInformation("Bir sonraki haber çekme: {NextRun}", nextRun);
                    await Task.Delay(delay, stoppingToken);

                    // Sabah 9'da çalışacak kod
                    _logger.LogInformation("Saat 09:00, haberler çekiliyor...");
                    // Henüz haber çekme mantığı eklemedik, sonraki adımda ekleyeceğiz
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("NewsFetcherService durduruluyor.");
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "NewsFetcherService çalışırken hata oluştu.");
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // Hata sonrası kısa bekleme
                }
            }
        }
    }
}