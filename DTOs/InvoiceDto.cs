using System.ComponentModel.DataAnnotations;

namespace InvoiceSystem.DTOs
{
    public class InvoiceDto
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? AcceptedDate { get; set; }
        public int AssignedUserId { get; set; }
        public int CreatedByAdminId { get; set; }
        public string AssignedUserName { get; set; } = string.Empty;
        public List<InvoiceItemDto> Items { get; set; } = new();
    }

    public class CreateInvoiceDto
    {
        [Required]
        [StringLength(200)]
        public string CustomerName { get; set; } = string.Empty;

        [Required]
        public DateTime DueDate { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int AssignedUserId { get; set; }

        [Required]
        [MinLength(1)]
        public List<CreateInvoiceItemDto> Items { get; set; } = new();
    }

    public class InvoiceItemDto
    {
        public string Description { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class CreateInvoiceItemDto
    {
        [Required]
        [StringLength(200)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal UnitPrice { get; set; }
    }

    public class RejectInvoiceDto
    {
        [Required]
        [StringLength(500)]
        public string Reason { get; set; } = string.Empty;
    }
}