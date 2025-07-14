using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using AINewsEngine.Data;
using AINewsEngine.Models;

namespace AINewsEngine.Controllers
{
    [Authorize] // Bu controller'a sadece giriş yapmış kullanıcılar erişebilir.
    [Route("api/[controller]")]
    [ApiController]
    public class YerIsaretleriController : ControllerBase
    {
        private readonly VeritabaniContext _context;

        public YerIsaretleriController(VeritabaniContext context)
        {
            _context = context;
        }

        // GET: api/YerIsaretleri
        // Giriş yapmış kullanıcının kaydettiği tüm haberleri döndürür.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Haber>>> GetYerIsaretliHaberler()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            var haberler = await _context.YerIsaretleri
                .Where(y => y.KullaniciId == userId)
                .Select(y => y.Haber) // Sadece ilişkili Haber nesnelerini seç
                .ToListAsync();

            return Ok(haberler);
        }

        // POST: api/YerIsaretleri/5
        // Belirtilen ID'ye sahip haberi kullanıcının yer işaretlerine ekler.
        [HttpPost("{haberId}")]
        public async Task<IActionResult> YerIsaretiEkle(int haberId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            // Haber var mı?
            var haberExists = await _context.Haberler.AnyAsync(h => h.Id == haberId);
            if (!haberExists) return NotFound(new { message = "Haber bulunamadı." });

            // Bu haber zaten daha önce eklenmiş mi?
            var isaretExists = await _context.YerIsaretleri
                .AnyAsync(y => y.KullaniciId == userId && y.HaberId == haberId);

            if (isaretExists)
            {
                return Ok(new { message = "Bu haber zaten yer işaretlerinizde." });
            }

            var yeniYerIsareti = new YerIsareti
            {
                KullaniciId = userId,
                HaberId = haberId
            };

            _context.YerIsaretleri.Add(yeniYerIsareti);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetYerIsaretliHaberler), new { id = yeniYerIsareti.Id }, yeniYerIsareti);
        }

        // DELETE: api/YerIsaretleri/5
        // Belirtilen ID'ye sahip haberi kullanıcının yer işaretlerinden kaldırır.
        [HttpDelete("{haberId}")]
        public async Task<IActionResult> YerIsaretiSil(int haberId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var yerIsareti = await _context.YerIsaretleri
                .FirstOrDefaultAsync(y => y.KullaniciId == userId && y.HaberId == haberId);

            if (yerIsareti == null)
            {
                return NotFound(new { message = "Bu haber yer işaretlerinizde bulunamadı." });
            }

            _context.YerIsaretleri.Remove(yerIsareti);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
