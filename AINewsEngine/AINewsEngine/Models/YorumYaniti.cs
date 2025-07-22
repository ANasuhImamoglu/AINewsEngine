using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AINewsEngine.Models
{
    public class YorumYaniti
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Icerik { get; set; } = string.Empty;

        public DateTime OlusturmaTarihi { get; set; } = DateTime.UtcNow;

        public bool Onaylandi { get; set; } = false;

        // Foreign Key - Yorum ile ilişki
        [Required]
        public int YorumId { get; set; }

        [ForeignKey("YorumId")]
        public Yorum? Yorum { get; set; }

        // Foreign Key - Kullanıcı ile ilişki
        [Required]
        public string KullaniciId { get; set; } = string.Empty;

        [ForeignKey("KullaniciId")]
        public Kullanici? Kullanici { get; set; }

        // Navigation Properties
        public virtual ICollection<YorumLike> Likes { get; set; } = new List<YorumLike>();

        // Computed Properties
        [NotMapped]
        public int LikeSayisi => Likes?.Count(l => l.IsLike) ?? 0;

        [NotMapped]
        public int DislikeSayisi => Likes?.Count(l => !l.IsLike) ?? 0;
    }
}
