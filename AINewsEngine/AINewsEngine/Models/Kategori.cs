using System.ComponentModel.DataAnnotations;

namespace AINewsEngine.Models
{
    public class Kategori
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Ad { get; set; } = string.Empty;

        // Bir kategorinin birden çok haberi olabilir (Navigation Property)
        public ICollection<Haber>? Haberler { get; set; }
    }
}