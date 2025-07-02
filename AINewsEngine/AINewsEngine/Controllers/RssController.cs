namespace AINewsEngine.Controllers;

using AINewsEngine.Service;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class RssController : ControllerBase
{
    private readonly IRssService _rssService;

    public RssController(IRssService rssService)
    {
        _rssService = rssService;
    }

    // POST: api/Rss/CekVeKaydet
    // Body'de gönderilen URL'deki haberleri çeker ve kaydeder.
    [HttpPost("CekVeKaydet")]
    public async Task<IActionResult> CekVeKaydet([FromBody] RssRequest request)
    {
        if (string.IsNullOrEmpty(request.FeedUrl) || !Uri.IsWellFormedUriString(request.FeedUrl, UriKind.Absolute))
        {
            return BadRequest("Geçerli bir RSS URL'i göndermelisiniz.");
        }

        try
        {
            var yeniHaberler = await _rssService.CekVeKaydetAsync(request.FeedUrl);

            if (!yeniHaberler.Any())
            {
                return Ok(new { message = "Tüm haberler zaten güncel. Yeni haber eklenmedi." });
            }

            return Ok(new { message = $"{yeniHaberler.Count} yeni haber başarıyla eklendi.", data = yeniHaberler });
        }
        catch (Exception ex)
        {
            // Servis katmanında yakalanamayan beklenmedik hatalar için
            return StatusCode(500, $"Sunucu hatası: {ex.Message}");
        }
    }
}

// Body'den gelecek JSON verisini karşılamak için basit bir kayıt (record)
public record RssRequest(string FeedUrl);

