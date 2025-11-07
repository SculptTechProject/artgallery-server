using artgallery_server.DTO.Admin;
using artgallery_server.Enum;
using artgallery_server.Infrastructure;
using artgallery_server.Models;
using Microsoft.EntityFrameworkCore;

namespace artgallery_server.Utils
{
    public class AdminSeeder
    {
        public static async Task SeedAsync(IServiceProvider sp)
        {
            using var scope = sp.CreateScope();
            var db  = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var username = Environment.GetEnvironmentVariable("ADMIN_USERNAME") ?? "admin";
            var pass = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");
            var hash= Environment.GetEnvironmentVariable("ADMIN_PASSWORD_HASH");
            var role = Roles.Admin.ToString();
            
            var exists = await db.Admins.AnyAsync(u => u.Username == username);
            if (exists) return;

            if (string.IsNullOrWhiteSpace(hash))
            {
                if (string.IsNullOrWhiteSpace(pass))
                    throw new InvalidOperationException("Set ADMIN_PASSWORD or ADMIN_PASSWORD_HASH for first run.");
                hash = BCrypt.Net.BCrypt.HashPassword(pass);
            }

            db.Admins.Add(new Admin { Username = username, PasswordHash = hash, Role = role });
            await db.SaveChangesAsync();
        }
    }
}
