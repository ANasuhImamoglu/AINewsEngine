using AINewsEngine.Data;
using AINewsEngine.Service;
using AINewsEngine.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// --- YENİ: CORS Politikası Adı ---
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

// Servisleri Konteynere Ekle

// --- YENİ: CORS Servisini Ekliyoruz ---
// Bu kod, geliştirme ortamında herhangi bir yerden gelen isteklere izin verir.
// Bu, Postman ve Swagger gibi araçların sorunsuz çalışmasını sağlar.
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.AllowAnyOrigin()
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                      });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<VeritabaniContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddHttpClient("RssClient", client => { client.Timeout = TimeSpan.FromSeconds(30); });

// LlmService artık OpenRouter'a göre çalıştığı için sistem sorunsuz çalışacaktır.
builder.Services.AddScoped<ILlmService, LlmService>();

// Kendi olu�turdu�umuz RSS servisini projemize tan�t�yoruz.
// Birisi IRssService istedi�inde, ona RssService'in bir �rne�ini ver.
builder.Services.AddScoped<IRssService, RssService>();

builder.Services.AddControllers();
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

//app.UseHttpsRedirection();

// --- YENİ: CORS Politikasını Uyguluyoruz ---
// Bu satır, UseRouting ve UseAuthorization arasında olmalıdır.
app.UseCors(MyAllowSpecificOrigins);

app.UseCors("AllowAngular"); // CORS kullanımı

// Yetkilendirme (Authorization) ara katman�n� etkinle�tirir.
app.UseAuthorization();

// Gelen istekleri do�ru controller'daki do�ru action'a y�nlendirmek i�in rotalar� e�ler.
app.MapControllers();

// 5. UYGULAMAYI �ALI�TIRMA
app.Run();
