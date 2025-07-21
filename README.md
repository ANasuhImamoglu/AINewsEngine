# 📰 AINewsEngine

## 🚀 Proje Amacı
AINewsEngine, haberleri RSS kaynaklarından çekip yapay zeka ile yeniden yazabilen, kullanıcı ve rol tabanlı güvenlik sunan, hem panel (web) hem de mobil uygulama ile kullanılabilen modern bir haber motorudur.

---

## ✨ Temel Özellikler
- 🔄 **RSS ile haber çekme** & LLM (Yapay Zeka) ile yeniden yazma
- 👤 **Kullanıcı ve rol yönetimi** (Admin, Moderator, User)
- 🔐 **JWT tabanlı kimlik doğrulama**
- 🛡️ **Rol bazlı endpoint koruması** (Admin/Moderator işlemleri)
- 🧪 **Swagger/OpenAPI ile kolay test**
- 🗄️ **SQLite veritabanı**
- 🖥️ **Angular tabanlı yönetim paneli** (ayrı dizinde)

---

## 🛠️ Kullanılan Teknolojiler
- ⚙️ .NET 8 (ASP.NET Core Web API)
- 🗃️ Entity Framework Core (SQLite)
- 👥 Microsoft.AspNetCore.Identity
- 🔑 JWT (JSON Web Token)
- 📑 Swashbuckle (Swagger UI)
- 🅰️ Angular (Panel için, ayrı dizinde)

---

## ⚡ Kurulum
1. **Projeyi klonlayın:**
   ```bash
   git clone <repo-url>
   cd AINewsEngine/AINewsEngine
   ```
2. **Bağımlılıkları yükleyin:**
   ```bash
   dotnet restore
   ```
3. **Veritabanını oluşturun ve migrate edin:**
   ```bash
   dotnet ef database update
   ```
4. **Projeyi başlatın:**
   ```bash
   dotnet run
   ```
   Uygulama varsayılan olarak [`http://localhost:5175`](http://localhost:5175) (veya launchSettings.json'daki port) üzerinden çalışır.

---

## 🧑‍💻 Kullanım
### 🧪 Swagger ile Test
1. Tarayıcıda [`http://localhost:5175/swagger`](http://localhost:5175/swagger) adresine gidin.
2. ➕ Kayıt olmak için `/api/Auth/register`, giriş yapmak için `/api/Auth/login` endpointlerini kullanın.
3. 🔑 Giriş yaptıktan sonra dönen JWT token'ı kopyalayın.
4. 🛡️ Swagger arayüzünde sağ üstteki **Authorize** (kilit) butonuna tıklayın ve `Bearer <token>` formatında token'ı girin.
5. 🚦 Artık korumalı endpointleri (ör. haber ekleme, silme, kategori ekleme, RSS çekme) test edebilirsiniz.

### 🛡️ Rol Bazlı Güvenlik
- 👀 **User** rolü: Sadece haberleri okuyabilir.
- 🛠️ **Admin/Moderator** rolleri: Haber ekleyebilir, güncelleyebilir, silebilir, onaylayabilir, kategori ve RSS işlemleri yapabilir.
- ❌ Korumalı endpointlere User rolüyle erişmek isterseniz **403 Forbidden** hatası alırsınız.

---

## ⚙️ Ortam Değişkenleri ve Ayarlar
- Veritabanı ve JWT ayarları `appsettings.json` dosyasından veya environment variable'lardan alınır.
- 🛡️ Admin kullanıcısı ilk çalıştırmada otomatik oluşturulur (kullanıcı adı/şifre: `Admin123`/`Admin123` veya user-secrets ile).

---

## 💡 Geliştirici Notları
- Kodda kritik endpointler `[Authorize(Roles = "Admin,Moderator")]` ile korunmuştur.
- Swagger ile test için önce login olup token'ı **Authorize** ile girmeniz gerekir.
- Rate limiting, CORS ve HTTPS gibi ek güvenlik önlemleri kodda örneklenmiştir.

---

## 📄 Lisans
MIT
