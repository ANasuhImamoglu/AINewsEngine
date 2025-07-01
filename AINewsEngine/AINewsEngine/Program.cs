using AINewsEngine.Data;
using Microsoft.EntityFrameworkCore;

// 1. UYGULAMAYI OLUÞTURMA VE YAPILANDIRMA
var builder = WebApplication.CreateBuilder(args);

// 2. SERVÝSLERÝ BAÐIMLILIK EKLEME (DEPENDENCY INJECTION) KONTEYNERÝNE EKLEME

// Connection string'i appsettings.json dosyasýndan alýyoruz.
// "DefaultConnection" isminin appsettings.json'daki ile ayný olduðundan emin olun.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// VeritabaniContext'i projenin servisleri arasýna ekliyoruz.
// Entity Framework Core'a bu context'in bir SQLite veritabaný kullanacaðýný söylüyoruz.
builder.Services.AddDbContext<VeritabaniContext>(options =>
    options.UseSqlite(connectionString));

// Controller'larý servis olarak ekliyoruz. Bu, API controller'larýnýn çalýþmasý için gereklidir.
builder.Services.AddControllers();

// API'yi test etmek ve belgelemek için Swagger/OpenAPI servisini ekliyoruz.
// Geliþtirme ortamýnda çok faydalýdýr.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 3. UYGULAMAYI DERLEME
var app = builder.Build();

// 4. HTTP ÝSTEK PÝPELINE'INI YAPILANDIRMA (Middleware)

// Sadece geliþtirme ortamýndayken Swagger'ý ve Swagger UI'ý etkinleþtiriyoruz.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Gelen HTTP isteklerini otomatik olarak HTTPS'e yönlendirir.
app.UseHttpsRedirection();

// Yetkilendirme (Authorization) ara katmanýný etkinleþtirir.
app.UseAuthorization();

// Gelen istekleri doðru controller'daki doðru action'a yönlendirmek için rotalarý eþler.
app.MapControllers();

// 5. UYGULAMAYI ÇALIÞTIRMA
app.Run();
