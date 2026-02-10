using InvoiceSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace InvoiceSystem.Data
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>());

            // Check if we already have data
            if (context.Users.Any())
            {
                return; // Database has been seeded
            }

            // Create Admin User
            var admin = new User
            {
                Username = "admin",
                Email = "admin@invoicesystem.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                Role = "Admin",
                CreatedAt = DateTime.UtcNow
            };

            context.Users.Add(admin);
            context.SaveChanges();

            // Create Regular User 1
            var user1 = new User
            {
                Username = "john_doe",
                Email = "john@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("user123"),
                Role = "User",
                CreatedAt = DateTime.UtcNow
            };

            context.Users.Add(user1);
            context.SaveChanges();

            // Create Regular User 2
            var user2 = new User
            {
                Username = "jane_smith",
                Email = "jane@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("user123"),
                Role = "User",
                CreatedAt = DateTime.UtcNow
            };

            context.Users.Add(user2);
            context.SaveChanges();

            // Create Sample Invoices
            var invoice1 = new Invoice
            {
                InvoiceNumber = "INV-2024-001",
                CustomerName = "Acme Corporation",
                IssueDate = DateTime.UtcNow.AddDays(-10),
                DueDate = DateTime.UtcNow.AddDays(20),
                Status = "Pending",
                AssignedUserId = user1.Id,
                CreatedByAdminId = admin.Id,
                Items = new List<InvoiceItem>
                {
                    new InvoiceItem
                    {
                        Description = "Web Development Services",
                        Quantity = 40,
                        UnitPrice = 50.00m,
                        TotalPrice = 2000.00m
                    },
                    new InvoiceItem
                    {
                        Description = "Hosting Services (Annual)",
                        Quantity = 1,
                        UnitPrice = 500.00m,
                        TotalPrice = 500.00m
                    }
                }
            };
            invoice1.TotalAmount = invoice1.Items.Sum(i => i.TotalPrice);

            var invoice2 = new Invoice
            {
                InvoiceNumber = "INV-2024-002",
                CustomerName = "Tech Solutions Inc.",
                IssueDate = DateTime.UtcNow.AddDays(-5),
                DueDate = DateTime.UtcNow.AddDays(25),
                Status = "Pending",
                AssignedUserId = user2.Id,
                CreatedByAdminId = admin.Id,
                Items = new List<InvoiceItem>
                {
                    new InvoiceItem
                    {
                        Description = "Software Consultation",
                        Quantity = 10,
                        UnitPrice = 100.00m,
                        TotalPrice = 1000.00m
                    }
                }
            };
            invoice2.TotalAmount = invoice2.Items.Sum(i => i.TotalPrice);

            var invoice3 = new Invoice
            {
                InvoiceNumber = "INV-2024-003",
                CustomerName = "Global Enterprises",
                IssueDate = DateTime.UtcNow.AddDays(-15),
                DueDate = DateTime.UtcNow.AddDays(-5),
                Status = "Overdue",
                AssignedUserId = user1.Id,
                CreatedByAdminId = admin.Id,
                Items = new List<InvoiceItem>
                {
                    new InvoiceItem
                    {
                        Description = "Mobile App Development",
                        Quantity = 80,
                        UnitPrice = 75.00m,
                        TotalPrice = 6000.00m
                    },
                    new InvoiceItem
                    {
                        Description = "UI/UX Design",
                        Quantity = 20,
                        UnitPrice = 60.00m,
                        TotalPrice = 1200.00m
                    }
                }
            };
            invoice3.TotalAmount = invoice3.Items.Sum(i => i.TotalPrice);

            context.Invoices.AddRange(invoice1, invoice2, invoice3);
            context.SaveChanges();

            Console.WriteLine("Database seeded successfully!");
            Console.WriteLine($"Admin credentials - Username: admin, Password: admin123");
            Console.WriteLine($"User credentials - Username: john_doe, Password: user123");
            Console.WriteLine($"User credentials - Username: jane_smith, Password: user123");
        }
    }
}