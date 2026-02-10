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
        public int AssignedUserId { get; set; }
        public string? AssignedUserName { get; set; }
        public List<InvoiceItemDto> Items { get; set; } = new List<InvoiceItemDto>();
    }

    public class InvoiceItemDto
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class CreateInvoiceDto
    {
        [Required]
        public string CustomerName { get; set; } = string.Empty;

        [Required]
        public DateTime DueDate { get; set; }

        [Required]
        public int AssignedUserId { get; set; }

        [Required]
        public List<CreateInvoiceItemDto> Items { get; set; } = new List<CreateInvoiceItemDto>();
    }

    public class CreateInvoiceItemDto
    {
        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal UnitPrice { get; set; }
    }

    public class UpdateInvoiceStatusDto
    {
        [Required]
        public string Status { get; set; } = string.Empty;
    }
}