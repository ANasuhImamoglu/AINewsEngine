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

        // Yorumlar sistemi tabloları
        public DbSet<Yorum> Yorumlar { get; set; }
        public DbSet<YorumYaniti> YorumYanitlari { get; set; }
        public DbSet<YorumLike> YorumLikes { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // YerIsaretleri tablosu için birincil anahtarın (primary key)
            // KullaniciId ve HaberId'den oluştuğunu belirtebiliriz (opsiyonel ama iyi pratik).
            // Ancak basitlik adına şimdilik ayrı bir Id alanı kullanıyoruz.

            // Yorumlar için ilişkileri yapılandır
            builder.Entity<Yorum>()
                .HasOne(y => y.Haber)
                .WithMany()
                .HasForeignKey(y => y.HaberId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Yorum>()
                .HasOne(y => y.Kullanici)
                .WithMany()
                .HasForeignKey(y => y.KullaniciId)
                .OnDelete(DeleteBehavior.Restrict);

            // Yorum yanıtları için ilişkileri yapılandır
            builder.Entity<YorumYaniti>()
                .HasOne(yy => yy.Yorum)
                .WithMany(y => y.Yanitlar)
                .HasForeignKey(yy => yy.YorumId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<YorumYaniti>()
                .HasOne(yy => yy.Kullanici)
                .WithMany()
                .HasForeignKey(yy => yy.KullaniciId)
                .OnDelete(DeleteBehavior.Restrict);

            // YorumLike için ilişkileri yapılandır
            builder.Entity<YorumLike>()
                .HasOne(yl => yl.Yorum)
                .WithMany(y => y.Likes)
                .HasForeignKey(yl => yl.YorumId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<YorumLike>()
                .HasOne(yl => yl.YorumYaniti)
                .WithMany(yy => yy.Likes)
                .HasForeignKey(yl => yl.YorumYanitiId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<YorumLike>()
                .HasOne(yl => yl.Kullanici)
                .WithMany()
                .HasForeignKey(yl => yl.KullaniciId)
                .OnDelete(DeleteBehavior.Restrict);

            // Bir kullanıcı aynı yoruma veya yanıta sadece bir kez like/dislike verebilir
            builder.Entity<YorumLike>()
                .HasIndex(yl => new { yl.KullaniciId, yl.YorumId })
                .IsUnique()
                .HasFilter("[YorumId] IS NOT NULL");

            builder.Entity<YorumLike>()
                .HasIndex(yl => new { yl.KullaniciId, yl.YorumYanitiId })
                .IsUnique()
                .HasFilter("[YorumYanitiId] IS NOT NULL");
        }
    }
}
