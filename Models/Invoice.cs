using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvoiceSystem.Models
{
    public class Invoice
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string InvoiceNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string CustomerName { get; set; } = string.Empty;

        [Required]
        public DateTime IssueDate { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Pending"; // "Pending", "Accepted", "Rejected"

        public DateTime? AcceptedDate { get; set; }

        // Foreign key to assigned user
        public int AssignedUserId { get; set; }

        // Foreign key to creator (admin)
        public int CreatedByAdminId { get; set; }

        // Navigation properties
        [ForeignKey("AssignedUserId")]
        public User AssignedUser { get; set; } = null!;

        [ForeignKey("CreatedByAdminId")]
        public User CreatedByAdmin { get; set; } = null!;

        // Invoice items (one-to-many)
        public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
    }
}