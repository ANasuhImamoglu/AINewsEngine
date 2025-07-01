namespace AINewsEngine.Models;
using System.ComponentModel.DataAnnotations;

// Bu dosya genellikle projenizin "Models" klasöründe yer alır.
public class Haber
{
    // Id INTEGER PRIMARY KEY AUTOINCREMENT
    [Key]
    public int Id { get; set; }

    // Baslik TEXT NOT NULL
    [Required(ErrorMessage = "Haber başlığı boş bırakılamaz.")]
    public string Baslik { get; set; }

    // Icerik TEXT
    public string? Icerik { get; set; } // Null olabilir

    // ResimUrl TEXT
    public string? ResimUrl { get; set; } // Null olabilir

    // YayinTarihi DATETIME
    public DateTime YayinTarihi { get; set; }

    // Onaylandi BOOLEAN DEFAULT FALSE
    public bool Onaylandi { get; set; }

    // Veritabanına yeni bir kayıt eklenirken varsayılan değerleri atamak için constructor.
    public Haber()
    {
        Baslik = string.Empty; // Required alanlar için null hatası almamak adına
        YayinTarihi = DateTime.Now;
        Onaylandi = false;
    }
}

