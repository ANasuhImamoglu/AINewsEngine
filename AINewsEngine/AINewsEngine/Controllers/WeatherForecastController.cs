using AINewsEngine.Data;
using AINewsEngine.DTOs;
using AINewsEngine.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class HaberlerController : ControllerBase
{
    private readonly VeritabaniContext _context;

    public HaberlerController(VeritabaniContext context)
    {
        _context = context;
    }

    // PANEL API - GET: api/Haberler?page=1&pageSize=10&kategoriId=1
    // Admin paneli için - TÜM haberleri gösterir (onaylanmamýþ dahil)
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    [HttpGet]
    public async Task<ActionResult<object>> GetHaberler(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? kategoriId = null)
    {
        // Sayfa numarasý 1'den baþlamalý
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100; // Maksimum limit

        // Temel sorgu: TÜM haberleri al (admin paneli için onaylanmamýþ olanlar da görünmeli)
        IQueryable<Haber> query = _context.Haberler.AsNoTracking();

        // Kategori filtresi varsa uygula
        if (kategoriId.HasValue && kategoriId.Value > 0)
        {
            query = query.Where(h => h.KategoriId == kategoriId.Value);
        }

        // Toplam kayýt sayýsý
        var totalCount = await query.CountAsync();

        // Sayfalama ile veri çekme
        var haberler = await query
            .OrderByDescending(h => h.YayinTarihi) // En yeni haberler önce
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new
        {
            data = haberler,
            totalCount = totalCount
        });
    }

    // PANEL API - GET: api/Haberler/search?term=kelime&page=1&pageSize=10
    // Admin paneli için arama
    [HttpGet("search")]
    public async Task<ActionResult<object>> SearchHaberler(
        [FromQuery] string term,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        // Sayfa numarasý 1'den baþlamalý
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100; // Maksimum limit

        // Temel sorgu: TÜM haberleri al (admin paneli için)
        IQueryable<Haber> query = _context.Haberler.AsNoTracking();

        // Arama terimi varsa filtrele
        if (!string.IsNullOrWhiteSpace(term))
        {
            term = term.Trim().ToLower();
            query = query.Where(h =>
                h.Baslik.ToLower().Contains(term) ||
                h.Icerik.ToLower().Contains(term)
            );
        }

        // Toplam kayýt sayýsý (filtrelenmiþ)
        var totalCount = await query.CountAsync();

        // Sayfalama ile veri çekme
        var haberler = await query
            .OrderByDescending(h => h.YayinTarihi) // En yeni haberler önce
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new
        {
            data = haberler,
            totalCount = totalCount
        });
    }

    // FLUTTER API - GET: api/Haberler/paged?pageNumber=1&pageSize=10&kategoriId=1
    // Flutter için - SADECE onaylanmýþ haberleri gösterir
    [HttpGet("paged")]
    public async Task<ActionResult<PagedResult<Haber>>> GetPagedHaberler(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? kategoriId = null)
    {
        // Temel sorgu: Sadece onaylanmýþ haberleri al (Flutter için)
        var query = _context.Haberler
                            .Where(h => h.Onaylandi == true)
                            .AsNoTracking();
                           
        // Kategori filtresi varsa uygula
        if (kategoriId.HasValue && kategoriId.Value != 0)
        {
            query = query.Where(h => h.KategoriId == kategoriId.Value);
        }

        // Sýralamayý filtrelemeden sonra yapýyoruz
        query = query.OrderByDescending(h => h.YayinTarihi);

        // Toplam haber sayýsýný alýyoruz
        var totalItems = await query.CountAsync();

        // Veritabanýndan sadece ilgili sayfadaki haberleri çekiyoruz
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();


        foreach (var haber in items)
        {
            if (!string.IsNullOrEmpty(haber.ResimYolu))
            {
                // Örnek: "/images/haber1.jpg"
                haber.ResimYolu = $"/images/{haber.ResimYolu}";
            }
        }

        // Sayfa bilgilerini hesaplýyoruz
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var paginationInfo = new PaginationInfo
        {
            TotalItems = totalItems,
            PageSize = pageSize,
            PageNumber = pageNumber,
            TotalPages = totalPages
        };

        // Sayfalanmýþ sonucu oluþturup döndürüyoruz
        var pagedResult = new PagedResult<Haber>
        {
            Items = items,
            Pagination = paginationInfo
        };

        return Ok(pagedResult);
    }

    // PANEL API - Haber onaylama
    [HttpPut("{id}/approve")]
    public async Task<ActionResult<Haber>> ApproveHaber(int id)
    {
        var haber = await _context.Haberler.FindAsync(id);
        if (haber == null)
        {
            return NotFound();
        }

        haber.Onaylandi = true;
        await _context.SaveChangesAsync();

        return Ok(haber);
    }

    // PANEL API - Okunma sayýsýný artýrma
    [HttpPut("{id}/increment-read")]
    public async Task<ActionResult> IncrementReadCount(int id)
    {
        var haber = await _context.Haberler.FindAsync(id);
        if (haber == null)
        {
            return NotFound();
        }

        haber.OkunmaSayisi++;
        await _context.SaveChangesAsync();

        return Ok();
    }

    // PANEL API - Týklanma sayýsýný artýrma  
    [HttpPut("{id}/increment-click")]
    public async Task<ActionResult> IncrementClickCount(int id)
    {
        var haber = await _context.Haberler.FindAsync(id);
        if (haber == null)
        {
            return NotFound();
        }

        haber.TiklanmaSayisi++;
        await _context.SaveChangesAsync();

        return Ok();
    }

    // FLUTTER API - GET: api/Haberler/5
    // ID'ye göre tek bir haber getirir
    [HttpGet("{id}")]
    public async Task<ActionResult<Haber>> GetHaber(int id)
    {
        var haber = await _context.Haberler
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync(h => h.Id == id);

        if (haber == null)
        {
            return NotFound();
        }

        return haber;
    }

    // FLUTTER API - PUT: api/Haberler/5
    // Mevcut bir haberi günceller
    [HttpPut("{id}")]
    public async Task<IActionResult> PutHaber(int id, Haber haber)
    {
        if (id != haber.Id)
        {
            return BadRequest();
        }

        _context.Entry(haber).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!HaberExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // FLUTTER API - POST: api/Haberler
    // Yeni bir haber oluþturur
    [HttpPost]
    public async Task<ActionResult<Haber>> PostHaber(Haber haber)
    {
        _context.Haberler.Add(haber);
        await _context.SaveChangesAsync();

        // Oluþturulan kaynaðýn konumunu header'da döndürür.
        return CreatedAtAction("GetHaber", new { id = haber.Id }, haber);
    }

    // FLUTTER API - DELETE: api/Haberler/5
    // Bir haberi siler
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteHaber(int id)
    {
        var haber = await _context.Haberler.FindAsync(id);
        if (haber == null)
        {
            return NotFound();
        }

        _context.Haberler.Remove(haber);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // FLUTTER API - Týklanma sayacýný artýrma
    [HttpPost("{id}/tiklandi")]
    public async Task<IActionResult> TiklanmaArtir(int id)
    {
        var haber = await _context.Haberler.FindAsync(id);
        if (haber == null)
        {
            return NotFound();
        }

        haber.TiklanmaSayisi++;
        await _context.SaveChangesAsync();

        return Ok();
    }

    // FLUTTER API - Okunma sayacýný artýrma
    [HttpPost("{id}/okundu")]
    public async Task<IActionResult> OkunmaArtir(int id)
    {
        var haber = await _context.Haberler.FindAsync(id);
        if (haber == null)
        {
            return NotFound();
        }

        haber.OkunmaSayisi++;
        await _context.SaveChangesAsync();

        return Ok();
    }

    // FLUTTER API - Haber onaylama (yetkilendirmeli)
    [Authorize(Roles = "Admin,Moderator")]
    [HttpPost("{id}/onayla")]
    public async Task<IActionResult> Onayla(int id)
    {
        var haber = await _context.Haberler.FindAsync(id);
        if (haber == null)
        {
            return NotFound();
        }

        haber.Onaylandi = true;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Haber baþarýyla onaylandý." });
    }

    // FLUTTER API - En çok okunan haberleri getir
    [HttpGet("most-read")]
    public async Task<ActionResult<List<Haber>>> GetMostReadNews()
    {
        var mostReadNews = await _context.Haberler
            .Where(h => h.Onaylandi == true)
            .OrderByDescending(h => h.OkunmaSayisi)
            .Take(10)
            .AsNoTracking()
            .ToListAsync();

        return Ok(mostReadNews);
    }

    // FLUTTER API - En çok týklanan haberleri getir
    [HttpGet("most-clicked")]
    public async Task<ActionResult<List<Haber>>> GetMostClickedNews()
    {
        var mostClickedNews = await _context.Haberler
            .Where(h => h.Onaylandi == true)
            .OrderByDescending(h => h.TiklanmaSayisi)
            .Take(10)
            .AsNoTracking()
            .ToListAsync();

        return Ok(mostClickedNews);
    }

    // Yardýmcý metod
    private bool HaberExists(int id)
    {
        return _context.Haberler.Any(e => e.Id == id);
    }
}
