using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AINewsEngine.Data;
using AINewsEngine.Models;
using AINewsEngine.DTOs;

namespace AINewsEngine.Controllers
{
    [ApiController]
    [Route("api/admin/[controller]")]
    [Authorize] // Bu kontrolöre sadece giriş yapmış kullanıcılar erişebilir
    public class YorumYonetimiController : ControllerBase
    {
        private readonly VeritabaniContext _context;

        public YorumYonetimiController(VeritabaniContext context)
        {
            _context = context;
        }

        // GET: api/admin/YorumYonetimi/bekleyen
        [HttpGet("bekleyen")]
        public async Task<ActionResult<PagedResult<YorumDto>>> GetBekleyenYorumlar(
            int sayfa = 1,
            int sayfaBoyutu = 20)
        {
            var query = _context.Yorumlar
                .Include(y => y.Kullanici)
                .Include(y => y.Haber)
                .Include(y => y.Yanitlar)
                .Include(y => y.Likes)
                .Where(y => !y.Onaylandi);

            var toplam = await query.CountAsync();

            var yorumlar = await query
                .OrderByDescending(y => y.OlusturmaTarihi)
                .Skip((sayfa - 1) * sayfaBoyutu)
                .Take(sayfaBoyutu)
                .Select(y => new YorumDto
                {
                    Id = y.Id,
                    Icerik = y.Icerik,
                    OlusturmaTarihi = y.OlusturmaTarihi,
                    Onaylandi = y.Onaylandi,
                    HaberId = y.HaberId,
                    KullaniciId = y.KullaniciId,
                    KullaniciAdi = y.Kullanici!.UserName ?? "Bilinmeyen",
                    LikeSayisi = y.Likes.Count(l => l.IsLike),
                    DislikeSayisi = y.Likes.Count(l => !l.IsLike),
                    YanitSayisi = y.Yanitlar.Count,
                    KullaniciLikeDurumu = null,
                    Yanitlar = new List<YorumYanitiDto>()
                })
                .ToListAsync();

            return Ok(new PagedResult<YorumDto>
            {
                Items = yorumlar,
                Pagination = new PaginationInfo
                {
                    TotalItems = toplam,
                    PageNumber = sayfa,
                    PageSize = sayfaBoyutu,
                    TotalPages = (int)Math.Ceiling((double)toplam / sayfaBoyutu)
                }
            });
        }

        // GET: api/admin/YorumYonetimi/bekleyen-yanitlar
        [HttpGet("bekleyen-yanitlar")]
        public async Task<ActionResult<PagedResult<YorumYanitiDto>>> GetBekleyenYanitlar(
            int sayfa = 1,
            int sayfaBoyutu = 20)
        {
            var query = _context.YorumYanitlari
                .Include(yy => yy.Kullanici)
                .Include(yy => yy.Yorum)
                    .ThenInclude(y => y.Haber)
                .Include(yy => yy.Likes)
                .Where(yy => !yy.Onaylandi);

            var toplam = await query.CountAsync();

            var yanitlar = await query
                .OrderByDescending(yy => yy.OlusturmaTarihi)
                .Skip((sayfa - 1) * sayfaBoyutu)
                .Take(sayfaBoyutu)
                .Select(yy => new YorumYanitiDto
                {
                    Id = yy.Id,
                    Icerik = yy.Icerik,
                    OlusturmaTarihi = yy.OlusturmaTarihi,
                    Onaylandi = yy.Onaylandi,
                    YorumId = yy.YorumId,
                    KullaniciId = yy.KullaniciId,
                    KullaniciAdi = yy.Kullanici!.UserName ?? "Bilinmeyen",
                    LikeSayisi = yy.Likes.Count(l => l.IsLike),
                    DislikeSayisi = yy.Likes.Count(l => !l.IsLike),
                    KullaniciLikeDurumu = null
                })
                .ToListAsync();

            return Ok(new PagedResult<YorumYanitiDto>
            {
                Items = yanitlar,
                Pagination = new PaginationInfo
                {
                    TotalItems = toplam,
                    PageNumber = sayfa,
                    PageSize = sayfaBoyutu,
                    TotalPages = (int)Math.Ceiling((double)toplam / sayfaBoyutu)
                }
            });
        }

        // PUT: api/admin/YorumYonetimi/yorum/{id}/onayla
        [HttpPut("yorum/{id}/onayla")]
        public async Task<ActionResult> YorumOnayla(int id)
        {
            var yorum = await _context.Yorumlar.FindAsync(id);
            if (yorum == null)
            {
                return NotFound("Yorum bulunamadı.");
            }

            yorum.Onaylandi = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Yorum başarıyla onaylandı." });
        }

        // PUT: api/admin/YorumYonetimi/yanit/{id}/onayla
        [HttpPut("yanit/{id}/onayla")]
        public async Task<ActionResult> YorumYanitiOnayla(int id)
        {
            var yanit = await _context.YorumYanitlari.FindAsync(id);
            if (yanit == null)
            {
                return NotFound("Yanıt bulunamadı.");
            }

            yanit.Onaylandi = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Yanıt başarıyla onaylandı." });
        }

        // DELETE: api/admin/YorumYonetimi/yorum/{id}/reddet
        [HttpDelete("yorum/{id}/reddet")]
        public async Task<ActionResult> YorumReddet(int id)
        {
            var yorum = await _context.Yorumlar.FindAsync(id);
            if (yorum == null)
            {
                return NotFound("Yorum bulunamadı.");
            }

            _context.Yorumlar.Remove(yorum);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Yorum başarıyla reddedildi ve silindi." });
        }

        // DELETE: api/admin/YorumYonetimi/yanit/{id}/reddet
        [HttpDelete("yanit/{id}/reddet")]
        public async Task<ActionResult> YorumYanitiReddet(int id)
        {
            var yanit = await _context.YorumYanitlari.FindAsync(id);
            if (yanit == null)
            {
                return NotFound("Yanıt bulunamadı.");
            }

            _context.YorumYanitlari.Remove(yanit);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Yanıt başarıyla reddedildi ve silindi." });
        }

        // GET: api/admin/YorumYonetimi/istatistikler
        [HttpGet("istatistikler")]
        public async Task<ActionResult> GetYorumIstatistikleri()
        {
            var toplamYorum = await _context.Yorumlar.CountAsync();
            var onaylananYorum = await _context.Yorumlar.CountAsync(y => y.Onaylandi);
            var bekleyenYorum = await _context.Yorumlar.CountAsync(y => !y.Onaylandi);

            var toplamYanit = await _context.YorumYanitlari.CountAsync();
            var onaylananYanit = await _context.YorumYanitlari.CountAsync(yy => yy.Onaylandi);
            var bekleyenYanit = await _context.YorumYanitlari.CountAsync(yy => !yy.Onaylandi);

            var toplamLike = await _context.YorumLikes.CountAsync(yl => yl.IsLike);
            var toplamDislike = await _context.YorumLikes.CountAsync(yl => !yl.IsLike);

            var bugunYorum = await _context.Yorumlar
                .CountAsync(y => y.OlusturmaTarihi.Date == DateTime.UtcNow.Date);

            return Ok(new
            {
                Yorumlar = new
                {
                    Toplam = toplamYorum,
                    Onaylanan = onaylananYorum,
                    Bekleyen = bekleyenYorum
                },
                Yanitlar = new
                {
                    Toplam = toplamYanit,
                    Onaylanan = onaylananYanit,
                    Bekleyen = bekleyenYanit
                },
                Begeniler = new
                {
                    ToplamLike = toplamLike,
                    ToplamDislike = toplamDislike
                },
                BugunYeniYorum = bugunYorum
            });
        }

        // GET: api/admin/YorumYonetimi/kullanici/{kullaniciId}/yorumlar
        [HttpGet("kullanici/{kullaniciId}/yorumlar")]
        public async Task<ActionResult<PagedResult<YorumDto>>> GetKullaniciYorumlari(
            string kullaniciId,
            int sayfa = 1,
            int sayfaBoyutu = 10)
        {
            var query = _context.Yorumlar
                .Include(y => y.Kullanici)
                .Include(y => y.Haber)
                .Include(y => y.Yanitlar)
                .Include(y => y.Likes)
                .Where(y => y.KullaniciId == kullaniciId);

            var toplam = await query.CountAsync();

            var yorumlar = await query
                .OrderByDescending(y => y.OlusturmaTarihi)
                .Skip((sayfa - 1) * sayfaBoyutu)
                .Take(sayfaBoyutu)
                .Select(y => new YorumDto
                {
                    Id = y.Id,
                    Icerik = y.Icerik,
                    OlusturmaTarihi = y.OlusturmaTarihi,
                    Onaylandi = y.Onaylandi,
                    HaberId = y.HaberId,
                    KullaniciId = y.KullaniciId,
                    KullaniciAdi = y.Kullanici!.UserName ?? "Bilinmeyen",
                    LikeSayisi = y.Likes.Count(l => l.IsLike),
                    DislikeSayisi = y.Likes.Count(l => !l.IsLike),
                    YanitSayisi = y.Yanitlar.Count,
                    KullaniciLikeDurumu = null,
                    Yanitlar = new List<YorumYanitiDto>()
                })
                .ToListAsync();

            return Ok(new PagedResult<YorumDto>
            {
                Items = yorumlar,
                Pagination = new PaginationInfo
                {
                    TotalItems = toplam,
                    PageNumber = sayfa,
                    PageSize = sayfaBoyutu,
                    TotalPages = (int)Math.Ceiling((double)toplam / sayfaBoyutu)
                }
            });
        }
    }
}
