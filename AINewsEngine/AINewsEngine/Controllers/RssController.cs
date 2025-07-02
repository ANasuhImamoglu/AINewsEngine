// ... using ifadeleri ...

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

    [HttpPost("CekVeKaydet")]
    public async Task<IActionResult> CekVeKaydet([FromBody] RssRequest request)
    {
        if (string.IsNullOrEmpty(request.FeedUrl) || !Uri.IsWellFormedUriString(request.FeedUrl, UriKind.Absolute) || request.KategoriId <= 0)
        {
            return BadRequest(new { message = "Geçerli bir 'FeedUrl' ve sıfırdan büyük bir 'KategoriId' göndermelisiniz." });
        }
        // ...
        var yeniHaberler = await _rssService.CekVeKaydetAsync(request.FeedUrl, request.KategoriId);
        // ...
        return Ok(new { message = $"{yeniHaberler.Count} yeni haber başarıyla eklendi.", data = yeniHaberler });
    }
}

// DEĞİŞİKLİK: Kategori adı yerine KategoriId alıyoruz.
public record RssRequest(string FeedUrl, int KategoriId);
