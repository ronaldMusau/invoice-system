using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvoiceSystem.Models
{
    public class InvoiceItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        // Foreign key to invoice
        public int InvoiceId { get; set; }

        // Navigation property
        [ForeignKey("InvoiceId")]
        public Invoice Invoice { get; set; } = null!;
    }
}