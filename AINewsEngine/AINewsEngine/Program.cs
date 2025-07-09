using AINewsEngine.Data;
using AINewsEngine.Models;
using AINewsEngine.Service;
using AINewsEngine.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --- CORS Politikası ---
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins, policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

// --- Veritabanı ve Diğer Servisler ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<VeritabaniContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddHttpClient("RssClient", client => { client.Timeout = TimeSpan.FromSeconds(30); });

builder.Services.AddScoped<ILlmService, LlmService>();
builder.Services.AddScoped<IRssService, RssService>();

// --- IDENTITY ve AUTHENTICATION YAPILANDIRMASI ---

// 1. Identity Servisini Ekleme
// Kullanici ve IdentityRole sınıflarını kullanarak Identity sistemini kuruyoruz.
// Şifre kuralları gibi ayarları burada daha esnek hale getirebiliriz.
builder.Services.AddIdentity<Kullanici, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<VeritabaniContext>()
.AddDefaultTokenProviders();

// 2. JWT Authentication'ı Ekleme
// API'mize gelen isteklerdeki "Bearer" token'larını nasıl doğrulayacağını öğretiyoruz.
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = false, // Yayıncıyı doğrulama (şimdilik kapalı)
        ValidateAudience = false, // Dinleyiciyi doğrulama (şimdilik kapalı)
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
});

// ----------------------------------------------------

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await SeedData.Initialize(services, builder.Configuration);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Veritabanını tohumlarken bir hata oluştu.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection(); // HTTPS yönlendirmesi kapalı kalmalı

app.UseCors(MyAllowSpecificOrigins);

// YENİ: Authentication middleware'ini ekliyoruz.
// Bu, UseAuthorization'dan ÖNCE gelmelidir.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
