using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AINewsEngine.Data;
using AINewsEngine.Models;

[Route("api/[controller]")]
[ApiController]
public class KategorilerController : ControllerBase
{
    private readonly VeritabaniContext _context;

    public KategorilerController(VeritabaniContext context)
    {
        _context = context;
    }

    // GET: api/Kategoriler
    // Tüm kategorileri listeler.
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Kategori>>> GetKategoriler()
    {
        return await _context.Kategoriler.ToListAsync();
    }

    // POST: api/Kategoriler
    // Yeni bir kategori oluşturur.
    [HttpPost]
    public async Task<ActionResult<Kategori>> PostKategori(Kategori kategori)
    {
        _context.Kategoriler.Add(kategori);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetKategoriler), new { id = kategori.Id }, kategori);
    }
}
