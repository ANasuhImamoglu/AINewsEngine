using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AINewsEngine.Models
{
    public class Haber
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Baslik { get; set; } = string.Empty;
        public string? Icerik { get; set; }
        public string? ResimUrl { get; set; }
        public DateTime YayinTarihi { get; set; }
        public bool Onaylandi { get; set; }

        // --- DEĞİŞİKLİKLER BURADA ---
        // Eski string Kategori alanı kaldırıldı.

    // Icerik TEXT
    public string? Icerik { get; set; } // Null olabilir

    // ResimUrl TEXT
    public string? ResimUrl { get; set; } // Null olabilir

    // YayinTarihi DATETIME
    public DateTime YayinTarihi { get; set; }

    // Onaylandi BOOLEAN DEFAULT FALSE
    public bool Onaylandi { get; set; } = false;

        // YENİ: Entity Framework'ün ilişkiyi anlaması için Navigation Property
        [ForeignKey("KategoriId")]
        public Kategori? Kategori { get; set; }
    }
}
