using AutoMapper;
using InvoiceSystem.Data;
using InvoiceSystem.DTOs;
using InvoiceSystem.Hubs;
using InvoiceSystem.Models;
using InvoiceSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace InvoiceSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class InvoicesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly PdfService _pdfService;
        private readonly IHubContext<NotificationHub> _hubContext;

        public InvoicesController(
            ApplicationDbContext context,
            IMapper mapper,
            PdfService pdfService,
            IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _mapper = mapper;
            _pdfService = pdfService;
            _hubContext = hubContext;
        }

        // GET: api/invoices (Admin gets all, User gets their own)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<InvoiceDto>>> GetInvoices()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            var userId = int.Parse(userIdClaim);
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "User";

            IQueryable<Invoice> query = _context.Invoices
                .Include(i => i.Items)
                .Include(i => i.AssignedUser);

            if (userRole != "Admin")
            {
                query = query.Where(i => i.AssignedUserId == userId);
            }

            var invoices = await query.OrderByDescending(i => i.IssueDate).ToListAsync();
            return Ok(_mapper.Map<List<InvoiceDto>>(invoices));
        }

        // GET: api/invoices/5
        [HttpGet("{id}")]
        public async Task<ActionResult<InvoiceDto>> GetInvoice(int id)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Items)
                .Include(i => i.AssignedUser)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
                return NotFound();

            // Check authorization
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "User";

            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            var userId = int.Parse(userIdClaim);

            if (userRole != "Admin" && invoice.AssignedUserId != userId)
                return Forbid();

            return Ok(_mapper.Map<InvoiceDto>(invoice));
        }

        // POST: api/invoices (Admin creates invoice)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<InvoiceDto>> CreateInvoice(CreateInvoiceDto createInvoiceDto)
        {
            try
            {
                var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(adminIdClaim))
                    return Unauthorized();

                var adminId = int.Parse(adminIdClaim);

                // Validate input
                if (string.IsNullOrWhiteSpace(createInvoiceDto.CustomerName))
                {
                    return BadRequest(new { message = "Customer name is required" });
                }

                if (createInvoiceDto.DueDate <= DateTime.UtcNow)
                {
                    return BadRequest(new { message = "Due date must be in the future" });
                }

                if (createInvoiceDto.Items == null || !createInvoiceDto.Items.Any())
                {
                    return BadRequest(new { message = "At least one invoice item is required" });
                }

                // Check if assigned user exists
                var assignedUser = await _context.Users.FindAsync(createInvoiceDto.AssignedUserId);
                if (assignedUser == null)
                {
                    return BadRequest(new { message = "Assigned user not found" });
                }

                // Create invoice
                var invoice = new Invoice
                {
                    InvoiceNumber = GenerateInvoiceNumber(),
                    CustomerName = createInvoiceDto.CustomerName.Trim(),
                    IssueDate = DateTime.UtcNow,
                    DueDate = createInvoiceDto.DueDate,
                    TotalAmount = 0,
                    Status = "Pending",
                    AssignedUserId = createInvoiceDto.AssignedUserId,
                    CreatedByAdminId = adminId,
                    Items = new List<InvoiceItem>()
                };

                // Calculate total amount and add items
                decimal totalAmount = 0;
                foreach (var itemDto in createInvoiceDto.Items)
                {
                    if (string.IsNullOrWhiteSpace(itemDto.Description))
                    {
                        return BadRequest(new { message = "Item description is required" });
                    }

                    if (itemDto.Quantity <= 0)
                    {
                        return BadRequest(new { message = "Item quantity must be greater than zero" });
                    }

                    if (itemDto.UnitPrice <= 0)
                    {
                        return BadRequest(new { message = "Item unit price must be greater than zero" });
                    }

                    var item = new InvoiceItem
                    {
                        Description = itemDto.Description.Trim(),
                        Quantity = itemDto.Quantity,
                        UnitPrice = itemDto.UnitPrice,
                        TotalPrice = itemDto.Quantity * itemDto.UnitPrice
                    };
                    totalAmount += item.TotalPrice;
                    invoice.Items.Add(item);
                }

                invoice.TotalAmount = totalAmount;

                _context.Invoices.Add(invoice);
                await _context.SaveChangesAsync();

                // Reload invoice with related data
                var createdInvoice = await _context.Invoices
                    .Include(i => i.Items)
                    .Include(i => i.AssignedUser)
                    .FirstOrDefaultAsync(i => i.Id == invoice.Id);

                // Create notification for assigned user
                var userNotification = new Notification
                {
                    Message = $"New invoice #{invoice.InvoiceNumber} has been assigned to you for {invoice.CustomerName}",
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false,
                    UserId = invoice.AssignedUserId
                };

                _context.Notifications.Add(userNotification);
                await _context.SaveChangesAsync();

                // Send real-time notification via SignalR to assigned user
                try
                {
                    await _hubContext.Clients.User(invoice.AssignedUserId.ToString())
                        .SendAsync("ReceiveNotification", userNotification.Message);
                }
                catch (Exception ex)
                {
                    // Log but don't fail the request if SignalR fails
                    Console.WriteLine($"SignalR notification to user failed: {ex.Message}");
                }

                return CreatedAtAction(
                    nameof(GetInvoice),
                    new { id = createdInvoice!.Id },
                    _mapper.Map<InvoiceDto>(createdInvoice)
                );
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Failed to create invoice: {ex.Message}" });
            }
        }

        // PUT: api/invoices/5/status
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateInvoiceStatus(int id, [FromBody] UpdateInvoiceStatusDto updateStatusDto)
        {
            try
            {
                var invoice = await _context.Invoices
                    .Include(i => i.AssignedUser)
                    .Include(i => i.CreatedByAdmin)
                    .FirstOrDefaultAsync(i => i.Id == id);

                if (invoice == null)
                    return NotFound();

                // Check authorization
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "User";

                if (string.IsNullOrEmpty(userIdClaim))
                    return Unauthorized();

                var userId = int.Parse(userIdClaim);

                if (userRole != "Admin" && invoice.AssignedUserId != userId)
                    return Forbid();

                // Validate status
                var validStatuses = new[] { "Pending", "Paid", "Overdue", "Cancelled" };
                if (!validStatuses.Contains(updateStatusDto.Status))
                {
                    return BadRequest(new { message = "Invalid status" });
                }

                // Get old status for notification message
                var oldStatus = invoice.Status;
                invoice.Status = updateStatusDto.Status;

                // Update accepted date if status changed to Paid
                if (updateStatusDto.Status == "Paid")
                {
                    invoice.AcceptedDate = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                // Create notifications based on who is updating
                if (userRole == "User")
                {
                    // User updated invoice status - notify admin
                    if (invoice.CreatedByAdminId.HasValue)
                    {
                        var adminNotification = new Notification
                        {
                            Message = $"Invoice #{invoice.InvoiceNumber} for {invoice.CustomerName} has been marked as {updateStatusDto.Status} by {invoice.AssignedUser.Username}",
                            CreatedAt = DateTime.UtcNow,
                            IsRead = false,
                            UserId = invoice.CreatedByAdminId.Value
                        };

                        _context.Notifications.Add(adminNotification);
                        await _context.SaveChangesAsync();

                        // Send real-time notification to admin
                        try
                        {
                            await _hubContext.Clients.User(invoice.CreatedByAdminId.Value.ToString())
                                .SendAsync("ReceiveNotification", adminNotification.Message);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"SignalR notification to admin failed: {ex.Message}");
                        }
                    }
                }
                else if (userRole == "Admin")
                {
                    // Admin updated invoice status - notify user
                    var userNotification = new Notification
                    {
                        Message = $"Invoice #{invoice.InvoiceNumber} for {invoice.CustomerName} has been updated from {oldStatus} to {updateStatusDto.Status} by admin",
                        CreatedAt = DateTime.UtcNow,
                        IsRead = false,
                        UserId = invoice.AssignedUserId
                    };

                    _context.Notifications.Add(userNotification);
                    await _context.SaveChangesAsync();

                    // Send real-time notification to user
                    try
                    {
                        await _hubContext.Clients.User(invoice.AssignedUserId.ToString())
                            .SendAsync("ReceiveNotification", userNotification.Message);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"SignalR notification to user failed: {ex.Message}");
                    }
                }

                return Ok(new { message = "Invoice status updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Failed to update invoice status: {ex.Message}" });
            }
        }

        // GET: api/invoices/5/download
        [HttpGet("{id}/download")]
        public async Task<IActionResult> DownloadInvoice(int id)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Items)
                .Include(i => i.AssignedUser)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
                return NotFound();

            // Check authorization
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "User";

            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            var userId = int.Parse(userIdClaim);

            if (userRole != "Admin" && invoice.AssignedUserId != userId)
                return Forbid();

            var pdfBytes = _pdfService.GenerateInvoicePdf(invoice);
            return File(pdfBytes, "application/pdf", $"Invoice_{invoice.InvoiceNumber}.pdf");
        }

        private string GenerateInvoiceNumber()
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var random = new Random().Next(1000, 9999);
            return $"INV-{timestamp}-{random}";
        }
    }
}