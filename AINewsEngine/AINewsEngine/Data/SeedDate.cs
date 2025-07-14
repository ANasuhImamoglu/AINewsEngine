using Microsoft.AspNetCore.Identity;
using AINewsEngine.Models;

namespace AINewsEngine.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<Kullanici>>();

            string[] roleNames = { "Admin", "Moderator", "User" };
            IdentityResult roleResult;

            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    // Rolleri oluştur
                    roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // User Secrets'tan veya appsettings'ten admin bilgilerini al
            var adminUsername = configuration["AdminUser:Username"] ?? "Admin123";
            var adminPassword = configuration["AdminUser:Password"] ?? "Admin123";

            // Varsayılan bir admin kullanıcısı oluştur
            var adminUser = await userManager.FindByNameAsync(adminUsername);
            if (adminUser == null)
            {
                adminUser = new Kullanici
                {
                    UserName = adminUsername,
                    Email = adminUsername,
                    EmailConfirmed = true // E-posta onayı gerektirmemesi için
                };
                await userManager.CreateAsync(adminUser, adminPassword);
                // Admin kullanıcısına "Admin" rolünü ata
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }
}