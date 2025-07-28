using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AINewsEngine.Models
{
    public class YorumLike
    {
        [Key]
        public int Id { get; set; }

        public bool IsLike { get; set; } // true = Like, false = Dislike

        public DateTime OlusturmaTarihi { get; set; } = DateTime.UtcNow;

        // Foreign Key - Kullanıcı ile ilişki
        [Required]
        public string KullaniciId { get; set; } = string.Empty;

        [ForeignKey("KullaniciId")]
        public Kullanici? Kullanici { get; set; }

        // Yorum veya YorumYaniti ile ilişki (birinden biri null olacak)
        public int? YorumId { get; set; }

        [ForeignKey("YorumId")]
        public Yorum? Yorum { get; set; }

        public int? YorumYanitiId { get; set; }

        [ForeignKey("YorumYanitiId")]
        public YorumYaniti? YorumYaniti { get; set; }
    }
}
