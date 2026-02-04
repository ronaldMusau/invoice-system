using System.ComponentModel.DataAnnotations;

namespace InvoiceSystem.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public byte[] PasswordHash { get; set; } = new byte[32];

        [Required]
        public byte[] PasswordSalt { get; set; } = new byte[32];

        [Required]
        [MaxLength(20)]
        public string Role { get; set; } = "User"; // "User" or "Admin"

        // Navigation property for invoices assigned to this user
        public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    }
}