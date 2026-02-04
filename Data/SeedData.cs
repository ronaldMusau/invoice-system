using InvoiceSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace InvoiceSystem.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                // Create database if it doesn't exist
                context.Database.EnsureCreated();

                // Check if users already exist
                if (!context.Users.Any())
                {
                    // Create password hash manually
                    var password = "Admin123!";
                    CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);

                    // Create sample admin
                    var admin = new User
                    {
                        Username = "admin",
                        Email = "admin@invoicesystem.com",
                        PasswordHash = passwordHash,
                        PasswordSalt = passwordSalt,
                        Role = "Admin"
                    };

                    context.Users.Add(admin);

                    // Create user1
                    CreatePasswordHash("User123!", out passwordHash, out passwordSalt);
                    var user1 = new User
                    {
                        Username = "user1",
                        Email = "user1@invoicesystem.com",
                        PasswordHash = passwordHash,
                        PasswordSalt = passwordSalt,
                        Role = "User"
                    };
                    context.Users.Add(user1);

                    // Create user2
                    CreatePasswordHash("User123!", out passwordHash, out passwordSalt);
                    var user2 = new User
                    {
                        Username = "user2",
                        Email = "user2@invoicesystem.com",
                        PasswordHash = passwordHash,
                        PasswordSalt = passwordSalt,
                        Role = "User"
                    };
                    context.Users.Add(user2);

                    await context.SaveChangesAsync();
                }
            }
        }

        private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }
    }
}