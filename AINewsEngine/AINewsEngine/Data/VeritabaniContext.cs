using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AINewsEngine.Models; // Bu satır çok önemli

namespace AINewsEngine.Data
{
    // DEĞİŞİKLİK: DbContext yerine IdentityDbContext kullanıyoruz.
    // Bu, Identity'nin kendi tablolarını (Users, Roles vb.) otomatik olarak yönetmesini sağlar.
    public class VeritabaniContext : IdentityDbContext<Kullanici>
    {
        public VeritabaniContext(DbContextOptions<VeritabaniContext> options) : base(options) { }

        public DbSet<Haber> Haberler { get; set; }
        public DbSet<Kategori> Kategoriler { get; set; }

        // YENİ: YerIsareti tablosunu DbContext'e ekliyoruz.
        public DbSet<YerIsareti> YerIsaretleri { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // YerIsaretleri tablosu için birincil anahtarın (primary key)
            // KullaniciId ve HaberId'den oluştuğunu belirtebiliriz (opsiyonel ama iyi pratik).
            // Ancak basitlik adına şimdilik ayrı bir Id alanı kullanıyoruz.
        }
    }
}
