namespace AINewsEngine.Data;

using AINewsEngine.Models;
using Microsoft.EntityFrameworkCore;

// Bu dosya genellikle projenizin "Data" veya "DAL" klasöründe yer alır.
public class VeritabaniContext : DbContext
{
    public VeritabaniContext(DbContextOptions<VeritabaniContext> options)
        : base(options)
    {
    }

    // "Haberler" tablosunu temsil eden DbSet
    public DbSet<Haber> Haberler { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Gerekirse burada daha detaylı model yapılandırması yapılabilir.
        // Örneğin, tablo ismini değiştirmek için:
        // modelBuilder.Entity<Haber>().ToTable("News");

        base.OnModelCreating(modelBuilder);
    }
}
