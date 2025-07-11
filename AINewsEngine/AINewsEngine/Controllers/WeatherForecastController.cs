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

    //Flutter i�in api k�sm� 

    // Mevcut endpoint'ler (�rne�in, GET /api/Haberler, POST /api/Haberler/fetch-rss)
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
    // T�m haberleri getirir.
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    [HttpGet]
    public async Task<ActionResult<PagedResult<Haber>>> GetHaberler(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? kategoriId = null) // Opsiyonel kategoriId parametresi
    {
        // Temel sorgu: Sadece onaylanm�� haberleri al.
        var query = _context.Haberler
                            .Where(h => h.Onaylandi == true)
                            .AsNoTracking();
                           

        // === DE����KL�K BURADA: Kategoriye g�re filtreleme ===
        // E�er bir kategoriId g�nderildiyse ve bu ID 0 de�ilse (yani "T�m�" sekmesi de�ilse),
        // sorguyu sadece o kategoriye ait haberleri i�erecek �ekilde filtrele.
        if (kategoriId.HasValue && kategoriId.Value != 0)
        {
            query = query.Where(h => h.KategoriId == kategoriId.Value);
        }

        // S�ralamay� filtrelemeden sonra yap�yoruz.
        query = query.OrderByDescending(h => h.YayinTarihi);

        // 1. Toplam haber say�s�n� al�yoruz.
        var totalItems = await query.CountAsync();

        // 2. Veritaban�ndan sadece ilgili sayfadaki haberleri �ekiyoruz.
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // 3. Sayfa bilgilerini hesapl�yoruz.
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var paginationInfo = new PaginationInfo
        {
            TotalItems = totalItems,
            PageSize = pageSize,
            PageNumber = pageNumber,
            TotalPages = totalPages
        };

        // 4. Sayfalanm�� sonucu olu�turup d�nd�r�yoruz.
        var pagedResult = new PagedResult<Haber>
        {
            Items = items,
            Pagination = paginationInfo
        };

        return Ok(pagedResult);
    }

    // GET: api/Haberler/5
    // ID'ye g�re tek bir haber getirir.
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
    // Mevcut bir haberi g�nceller.
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
    // Yeni bir haber olu�turur.
    [HttpPost]
    public async Task<ActionResult<Haber>> PostHaber(Haber haber)
    {
        _context.Haberler.Add(haber);
        await _context.SaveChangesAsync();

        // Olu�turulan kayna��n konumunu header'da d�nd�r�r.
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

    // === YEN� ENDPOINT 1: T�klanma Sayac�n� Art�rma ===
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

        return Ok(); // Ba�ar�l� yan�t� d�ner
    }

    // === YEN� ENDPOINT 2: Okunma Sayac�n� Art�rma ===
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

        return Ok(); // Ba�ar�l� yan�t� d�ner
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

        return Ok(new { message = "Haber ba�ar�yla onayland�." });
    }

    private bool HaberExists(int id)
    {
        return _context.Haberler.Any(e => e.Id == id);
    }
}
