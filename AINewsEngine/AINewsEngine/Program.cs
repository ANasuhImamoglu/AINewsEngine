using AINewsEngine.Data;
using AINewsEngine.Service;
using Microsoft.EntityFrameworkCore;

// 1. UYGULAMAYI OLU�TURMA VE YAPILANDIRMA
var builder = WebApplication.CreateBuilder(args);

// 2. SERV�SLER� BA�IMLILIK EKLEME (DEPENDENCY INJECTION) KONTEYNER�NE EKLEME

// Connection string'i appsettings.json dosyas�ndan al�yoruz.
// "DefaultConnection" isminin appsettings.json'daki ile ayn� oldu�undan emin olun.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// VeritabaniContext'i projenin servisleri aras�na ekliyoruz.
// Entity Framework Core'a bu context'in bir SQLite veritaban� kullanaca��n� s�yl�yoruz.
builder.Services.AddDbContext<VeritabaniContext>(options =>
    options.UseSqlite(connectionString));

// Controller'lar� servis olarak ekliyoruz. Bu, API controller'lar�n�n �al��mas� i�in gereklidir.
builder.Services.AddControllers();

// HttpClientFactory'yi ekliyoruz. Bu, HTTP isteklerini y�netmek i�in en iyi yoldur.
builder.Services.AddHttpClient();

// LlmService artık OpenRouter'a göre çalıştığı için sistem sorunsuz çalışacaktır.
builder.Services.AddScoped<ILlmService, LlmService>();

// Kendi olu�turdu�umuz RSS servisini projemize tan�t�yoruz.
// Birisi IRssService istedi�inde, ona RssService'in bir �rne�ini ver.
builder.Services.AddScoped<IRssService, RssService>();

// API'yi test etmek ve belgelemek i�in Swagger/OpenAPI servisini ekliyoruz.
// Geli�tirme ortam�nda �ok faydal�d�r.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// CORS eklendi
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", builder =>
    {
        builder.WithOrigins("http://localhost:4200")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});


// 3. UYGULAMAYI DERLEME
var app = builder.Build();

// 4. HTTP �STEK P�PELINE'INI YAPILANDIRMA (Middleware)
 
// Sadece geli�tirme ortam�ndayken Swagger'� ve Swagger UI'� etkinle�tiriyoruz.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Gelen HTTP isteklerini otomatik olarak HTTPS'e y�nlendirir.
app.UseHttpsRedirection();

app.UseCors("AllowAngular"); // CORS kullanımı

// Yetkilendirme (Authorization) ara katman�n� etkinle�tirir.
app.UseAuthorization();

// Gelen istekleri do�ru controller'daki do�ru action'a y�nlendirmek i�in rotalar� e�ler.
app.MapControllers();

// 5. UYGULAMAYI �ALI�TIRMA
app.Run();
