using InvoiceSystem.Models;

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
        public List<InvoiceItemDto> Items { get; set; } = new();
    }

    public class CreateInvoiceDto
    {
        public string CustomerName { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public int AssignedUserId { get; set; }
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
        public string Description { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}