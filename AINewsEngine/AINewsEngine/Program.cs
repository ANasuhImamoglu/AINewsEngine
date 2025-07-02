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

builder.Services.AddScoped<ILlmService, LlmService>();
builder.Services.AddScoped<IRssService, RssService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// --- YENİ: CORS Politikasını Uyguluyoruz ---
// Bu satır, UseRouting ve UseAuthorization arasında olmalıdır.
app.UseCors(MyAllowSpecificOrigins);

app.UseAuthorization();

app.MapControllers();

app.Run();
