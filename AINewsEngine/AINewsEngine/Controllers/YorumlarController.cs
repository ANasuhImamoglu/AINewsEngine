using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AINewsEngine.Data;
using AINewsEngine.Models;
using AINewsEngine.DTOs;
using System.Security.Claims;

namespace AINewsEngine.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class YorumlarController : ControllerBase
    {
        private readonly VeritabaniContext _context;

        public YorumlarController(VeritabaniContext context)
        {
            _context = context;
        }

        // GET: api/Yorumlar/haber/{haberId}
        [HttpGet("haber/{haberId}")]
        public async Task<ActionResult<PagedResult<YorumDto>>> GetHaberYorumlari(
            int haberId,
            int sayfa = 1,
            int sayfaBoyutu = 10,
            bool sadeceonaylanan = true)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var query = _context.Yorumlar
                .Include(y => y.Kullanici)
                .Include(y => y.Yanitlar)
                    .ThenInclude(yy => yy.Kullanici)
                .Include(y => y.Yanitlar)
                    .ThenInclude(yy => yy.Likes)
                .Include(y => y.Likes)
                .Where(y => y.HaberId == haberId);

            if (sadeceonaylanan)
            {
                query = query.Where(y => y.Onaylandi);
            }

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
                    YanitSayisi = y.Yanitlar.Count(yy => sadeceonaylanan ? yy.Onaylandi : true),
                    KullaniciLikeDurumu = currentUserId != null ?
                        y.Likes.Where(l => l.KullaniciId == currentUserId).Select(l => (bool?)l.IsLike).FirstOrDefault() : null,
                    Yanitlar = y.Yanitlar
                        .Where(yy => sadeceonaylanan ? yy.Onaylandi : true)
                        .OrderBy(yy => yy.OlusturmaTarihi)
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
                            KullaniciLikeDurumu = currentUserId != null ?
                                yy.Likes.Where(l => l.KullaniciId == currentUserId).Select(l => (bool?)l.IsLike).FirstOrDefault() : null
                        }).ToList()
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

        // POST: api/Yorumlar
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<YorumDto>> YorumEkle([FromBody] YorumEkleDto dto)
        {
            var kullaniciId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (kullaniciId == null)
            {
                return Unauthorized();
            }

            // Haberin var olup olmadığını kontrol et
            var haberVarMi = await _context.Haberler.AnyAsync(h => h.Id == dto.HaberId);
            if (!haberVarMi)
            {
                return BadRequest("Belirtilen haber bulunamadı.");
            }

            var yorum = new Yorum
            {
                Icerik = dto.Icerik,
                HaberId = dto.HaberId,
                KullaniciId = kullaniciId,
                OlusturmaTarihi = DateTime.UtcNow,
                Onaylandi = false // Varsayılan olarak onay bekler
            };

            _context.Yorumlar.Add(yorum);
            await _context.SaveChangesAsync();

            // Eklenen yorumu döndür
            var eklenenYorum = await _context.Yorumlar
                .Include(y => y.Kullanici)
                .Where(y => y.Id == yorum.Id)
                .Select(y => new YorumDto
                {
                    Id = y.Id,
                    Icerik = y.Icerik,
                    OlusturmaTarihi = y.OlusturmaTarihi,
                    Onaylandi = y.Onaylandi,
                    HaberId = y.HaberId,
                    KullaniciId = y.KullaniciId,
                    KullaniciAdi = y.Kullanici!.UserName ?? "Bilinmeyen",
                    LikeSayisi = 0,
                    DislikeSayisi = 0,
                    YanitSayisi = 0,
                    KullaniciLikeDurumu = null,
                    Yanitlar = new List<YorumYanitiDto>()
                })
                .FirstOrDefaultAsync();

            return CreatedAtAction(nameof(GetYorum), new { id = yorum.Id }, eklenenYorum);
        }

        // POST: api/Yorumlar/{yorumId}/yanitlar
        [HttpPost("{yorumId}/yanitlar")]
        [Authorize]
        public async Task<ActionResult<YorumYanitiDto>> YorumYanitiEkle(int yorumId, [FromBody] YorumYanitiEkleDto dto)
        {
            var kullaniciId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (kullaniciId == null)
            {
                return Unauthorized();
            }

            // Yorumun var olup olmadığını kontrol et
            var yorumVarMi = await _context.Yorumlar.AnyAsync(y => y.Id == yorumId);
            if (!yorumVarMi)
            {
                return BadRequest("Belirtilen yorum bulunamadı.");
            }

            var yorumYaniti = new YorumYaniti
            {
                Icerik = dto.Icerik,
                YorumId = yorumId,
                KullaniciId = kullaniciId,
                OlusturmaTarihi = DateTime.UtcNow,
                Onaylandi = false // Varsayılan olarak onay bekler
            };

            _context.YorumYanitlari.Add(yorumYaniti);
            await _context.SaveChangesAsync();

            // Eklenen yanıtı döndür
            var eklenenYanit = await _context.YorumYanitlari
                .Include(yy => yy.Kullanici)
                .Where(yy => yy.Id == yorumYaniti.Id)
                .Select(yy => new YorumYanitiDto
                {
                    Id = yy.Id,
                    Icerik = yy.Icerik,
                    OlusturmaTarihi = yy.OlusturmaTarihi,
                    Onaylandi = yy.Onaylandi,
                    YorumId = yy.YorumId,
                    KullaniciId = yy.KullaniciId,
                    KullaniciAdi = yy.Kullanici!.UserName ?? "Bilinmeyen",
                    LikeSayisi = 0,
                    DislikeSayisi = 0,
                    KullaniciLikeDurumu = null
                })
                .FirstOrDefaultAsync();

            return CreatedAtAction(nameof(GetYorumYaniti), new { id = yorumYaniti.Id }, eklenenYanit);
        }

        // POST: api/Yorumlar/{yorumId}/like
        [HttpPost("{yorumId}/like")]
        [Authorize]
        public async Task<ActionResult> YorumLike(int yorumId, [FromBody] YorumLikeDto dto)
        {
            var kullaniciId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (kullaniciId == null)
            {
                return Unauthorized();
            }

            // Yorumun var olup olmadığını kontrol et
            var yorumVarMi = await _context.Yorumlar.AnyAsync(y => y.Id == yorumId);
            if (!yorumVarMi)
            {
                return BadRequest("Belirtilen yorum bulunamadı.");
            }

            // Mevcut like kaydını kontrol et
            var mevcutLike = await _context.YorumLikes
                .FirstOrDefaultAsync(yl => yl.YorumId == yorumId && yl.KullaniciId == kullaniciId);

            if (mevcutLike != null)
            {
                if (mevcutLike.IsLike == dto.IsLike)
                {
                    // Aynı işlem tekrarlanıyorsa like'ı kaldır
                    _context.YorumLikes.Remove(mevcutLike);
                }
                else
                {
                    // Farklı işlem yapılıyorsa güncelle
                    mevcutLike.IsLike = dto.IsLike;
                    mevcutLike.OlusturmaTarihi = DateTime.UtcNow;
                }
            }
            else
            {
                // Yeni like kaydı oluştur
                var yeniLike = new YorumLike
                {
                    YorumId = yorumId,
                    KullaniciId = kullaniciId,
                    IsLike = dto.IsLike,
                    OlusturmaTarihi = DateTime.UtcNow
                };
                _context.YorumLikes.Add(yeniLike);
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "İşlem başarılı" });
        }

        // POST: api/Yorumlar/yanitlar/{yanitId}/like
        [HttpPost("yanitlar/{yanitId}/like")]
        [Authorize]
        public async Task<ActionResult> YorumYanitiLike(int yanitId, [FromBody] YorumLikeDto dto)
        {
            var kullaniciId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (kullaniciId == null)
            {
                return Unauthorized();
            }

            // Yanıtın var olup olmadığını kontrol et
            var yanitVarMi = await _context.YorumYanitlari.AnyAsync(yy => yy.Id == yanitId);
            if (!yanitVarMi)
            {
                return BadRequest("Belirtilen yanıt bulunamadı.");
            }

            // Mevcut like kaydını kontrol et
            var mevcutLike = await _context.YorumLikes
                .FirstOrDefaultAsync(yl => yl.YorumYanitiId == yanitId && yl.KullaniciId == kullaniciId);

            if (mevcutLike != null)
            {
                if (mevcutLike.IsLike == dto.IsLike)
                {
                    // Aynı işlem tekrarlanıyorsa like'ı kaldır
                    _context.YorumLikes.Remove(mevcutLike);
                }
                else
                {
                    // Farklı işlem yapılıyorsa güncelle
                    mevcutLike.IsLike = dto.IsLike;
                    mevcutLike.OlusturmaTarihi = DateTime.UtcNow;
                }
            }
            else
            {
                // Yeni like kaydı oluştur
                var yeniLike = new YorumLike
                {
                    YorumYanitiId = yanitId,
                    KullaniciId = kullaniciId,
                    IsLike = dto.IsLike,
                    OlusturmaTarihi = DateTime.UtcNow
                };
                _context.YorumLikes.Add(yeniLike);
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "İşlem başarılı" });
        }

        // GET: api/Yorumlar/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<YorumDto>> GetYorum(int id)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var yorum = await _context.Yorumlar
                .Include(y => y.Kullanici)
                .Include(y => y.Yanitlar)
                    .ThenInclude(yy => yy.Kullanici)
                .Include(y => y.Yanitlar)
                    .ThenInclude(yy => yy.Likes)
                .Include(y => y.Likes)
                .Where(y => y.Id == id)
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
                    KullaniciLikeDurumu = currentUserId != null ?
                        y.Likes.Where(l => l.KullaniciId == currentUserId).Select(l => (bool?)l.IsLike).FirstOrDefault() : null,
                    Yanitlar = y.Yanitlar
                        .OrderBy(yy => yy.OlusturmaTarihi)
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
                            KullaniciLikeDurumu = currentUserId != null ?
                                yy.Likes.Where(l => l.KullaniciId == currentUserId).Select(l => (bool?)l.IsLike).FirstOrDefault() : null
                        }).ToList()
                })
                .FirstOrDefaultAsync();

            if (yorum == null)
            {
                return NotFound();
            }

            return Ok(yorum);
        }

        // GET: api/Yorumlar/yanitlar/{id}
        [HttpGet("yanitlar/{id}")]
        public async Task<ActionResult<YorumYanitiDto>> GetYorumYaniti(int id)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var yanit = await _context.YorumYanitlari
                .Include(yy => yy.Kullanici)
                .Include(yy => yy.Likes)
                .Where(yy => yy.Id == id)
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
                    KullaniciLikeDurumu = currentUserId != null ?
                        yy.Likes.Where(l => l.KullaniciId == currentUserId).Select(l => (bool?)l.IsLike).FirstOrDefault() : null
                })
                .FirstOrDefaultAsync();

            if (yanit == null)
            {
                return NotFound();
            }

            return Ok(yanit);
        }

        // DELETE: api/Yorumlar/{id}
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult> YorumSil(int id)
        {
            var kullaniciId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (kullaniciId == null)
            {
                return Unauthorized();
            }

            var yorum = await _context.Yorumlar.FindAsync(id);
            if (yorum == null)
            {
                return NotFound();
            }

            // Sadece kendi yorumunu silebilir (admin kontrolü eklenebilir)
            if (yorum.KullaniciId != kullaniciId)
            {
                return Forbid("Bu yorumu silme yetkiniz yok.");
            }

            _context.Yorumlar.Remove(yorum);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Yorum başarıyla silindi" });
        }

        // DELETE: api/Yorumlar/yanitlar/{id}
        [HttpDelete("yanitlar/{id}")]
        [Authorize]
        public async Task<ActionResult> YorumYanitiSil(int id)
        {
            var kullaniciId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (kullaniciId == null)
            {
                return Unauthorized();
            }

            var yanit = await _context.YorumYanitlari.FindAsync(id);
            if (yanit == null)
            {
                return NotFound();
            }

            // Sadece kendi yanıtını silebilir (admin kontrolü eklenebilir)
            if (yanit.KullaniciId != kullaniciId)
            {
                return Forbid("Bu yanıtı silme yetkiniz yok.");
            }

            _context.YorumYanitlari.Remove(yanit);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Yanıt başarıyla silindi" });
        }
    }
}
