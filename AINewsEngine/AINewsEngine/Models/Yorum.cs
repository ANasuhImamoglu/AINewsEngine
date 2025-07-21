using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AINewsEngine.Models
{
    public class Yorum
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Icerik { get; set; } = string.Empty;

        public DateTime OlusturmaTarihi { get; set; } = DateTime.UtcNow;

        public bool Onaylandi { get; set; } = false;

        // Foreign Key - Haber ile ilişki
        [Required]
        public int HaberId { get; set; }

        [ForeignKey("HaberId")]
        public Haber? Haber { get; set; }

        // Foreign Key - Kullanıcı ile ilişki
        [Required]
        public string KullaniciId { get; set; } = string.Empty;

        [ForeignKey("KullaniciId")]
        public Kullanici? Kullanici { get; set; }

        // Navigation Properties
        public virtual ICollection<YorumYaniti> Yanitlar { get; set; } = new List<YorumYaniti>();
        public virtual ICollection<YorumLike> Likes { get; set; } = new List<YorumLike>();

        // Computed Properties
        [NotMapped]
        public int LikeSayisi => Likes?.Count(l => l.IsLike) ?? 0;

        [NotMapped]
        public int DislikeSayisi => Likes?.Count(l => !l.IsLike) ?? 0;

        [NotMapped]
        public int YanitSayisi => Yanitlar?.Count ?? 0;
    }
}
