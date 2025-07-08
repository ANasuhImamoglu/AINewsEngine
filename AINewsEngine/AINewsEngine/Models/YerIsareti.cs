using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity; // Kullanici sınıfı için bu gerekebilir

namespace AINewsEngine.Models
{
    public class YerIsareti
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string KullaniciId { get; set; } = string.Empty;

        [Required]
        public int HaberId { get; set; }

        // Navigation Properties (İlişkileri belirtmek için)
        [ForeignKey("KullaniciId")]
        public Kullanici? Kullanici { get; set; }

        [ForeignKey("HaberId")]
        public Haber? Haber { get; set; }
    }
}