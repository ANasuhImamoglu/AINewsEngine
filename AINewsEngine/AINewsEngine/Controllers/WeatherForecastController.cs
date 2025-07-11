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

    //Flutter için api kýsmý 

    // Mevcut endpoint'ler (örneðin, GET /api/Haberler, POST /api/Haberler/fetch-rss)
    [HttpPut("{id}/approve")]
    public async Task<IActionResult> ApproveNews(int id)
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






    // GET: api/Haberler
    // Tüm haberleri getirir.
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    [HttpGet]
    public async Task<ActionResult<PagedResult<Haber>>> GetHaberler(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? kategoriId = null) // Opsiyonel kategoriId parametresi
    {
        // Temel sorgu: Sadece onaylanmýþ haberleri al.
        var query = _context.Haberler
                            .Where(h => h.Onaylandi == true)
                            .AsNoTracking();
                           

        // === DEÐÝÞÝKLÝK BURADA: Kategoriye göre filtreleme ===
        // Eðer bir kategoriId gönderildiyse ve bu ID 0 deðilse (yani "Tümü" sekmesi deðilse),
        // sorguyu sadece o kategoriye ait haberleri içerecek þekilde filtrele.
        if (kategoriId.HasValue && kategoriId.Value != 0)
        {
            query = query.Where(h => h.KategoriId == kategoriId.Value);
        }

        // Sýralamayý filtrelemeden sonra yapýyoruz.
        query = query.OrderByDescending(h => h.YayinTarihi);

        // 1. Toplam haber sayýsýný alýyoruz.
        var totalItems = await query.CountAsync();

        // 2. Veritabanýndan sadece ilgili sayfadaki haberleri çekiyoruz.
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // 3. Sayfa bilgilerini hesaplýyoruz.
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var paginationInfo = new PaginationInfo
        {
            TotalItems = totalItems,
            PageSize = pageSize,
            PageNumber = pageNumber,
            TotalPages = totalPages
        };

        // 4. Sayfalanmýþ sonucu oluþturup döndürüyoruz.
        var pagedResult = new PagedResult<Haber>
        {
            Items = items,
            Pagination = paginationInfo
        };

        return Ok(pagedResult);
    }

    // GET: api/Haberler/5
    // ID'ye göre tek bir haber getirir.
    [HttpGet("{id}")]
    public async Task<ActionResult<Haber>> GetHaber(int id)
    {
        var haber = await _context.Haberler
                                  .AsNoTracking() // Buraya da eklemek iyi bir pratiktir.
                                  .FirstOrDefaultAsync(h => h.Id == id);

        if (haber == null)
        {
            return NotFound();
        }

        return haber;
    }

    // PUT: api/Haberler/5
    // Mevcut bir haberi günceller.
    [HttpPut("{id}")]
    public async Task<IActionResult> PutHaber(int id, Haber haber)
    {
        if (id != haber.Id)
        {
            return BadRequest(); // 400 Bad Request
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

        return NoContent(); // 204 No Content
    }

    // POST: api/Haberler
    // Yeni bir haber oluþturur.
    [HttpPost]
    public async Task<ActionResult<Haber>> PostHaber(Haber haber)
    {
        _context.Haberler.Add(haber);
        await _context.SaveChangesAsync();

        // Oluþturulan kaynaðýn konumunu header'da döndürür.
        return CreatedAtAction("GetHaber", new { id = haber.Id }, haber);
    }

    // DELETE: api/Haberler/5
    // Bir haberi siler.
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

        return NoContent(); // 204 No Content
    }

    // === YENÝ ENDPOINT 1: Týklanma Sayacýný Artýrma ===
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

        return Ok(); // Baþarýlý yanýtý döner
    }

    // === YENÝ ENDPOINT 2: Okunma Sayacýný Artýrma ===
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

        return Ok(); // Baþarýlý yanýtý döner
    }

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

    private bool HaberExists(int id)
    {
        return _context.Haberler.Any(e => e.Id == id);
    }
}
