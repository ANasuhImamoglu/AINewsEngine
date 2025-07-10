using AINewsEngine.Data;
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
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Haber>>> GetHaberler()
    {
        return await _context.Haberler.ToListAsync();
    }

    // GET: api/Haberler/5
    // ID'ye göre tek bir haber getirir.
    [HttpGet("{id}")]
    public async Task<ActionResult<Haber>> GetHaber(int id)
    {
        var haber = await _context.Haberler.FindAsync(id);
        
        //burada flutter a gönder
        if (haber == null)
        {
            return NotFound(); // 404 Not Found
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
