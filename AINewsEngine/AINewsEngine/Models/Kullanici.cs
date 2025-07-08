using Microsoft.AspNetCore.Identity;

namespace AINewsEngine.Models
{
    // ASP.NET Core Identity'nin standart kullanıcı sınıfından kalıtım alıyoruz.
    // Bu bize Id, UserName, PasswordHash gibi alanları hazır olarak sunar.
    public class Kullanici : IdentityUser
    {
        // Gelecekte kullanıcıya özel alanlar eklemek isterseniz
        // (örneğin, Ad, Soyad) buraya ekleyebilirsiniz.
        // public string? Ad { get; set; }
    }
}
