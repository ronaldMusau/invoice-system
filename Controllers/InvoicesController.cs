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

            var invoices = await query.ToListAsync();
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
            var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(adminIdClaim))
                return Unauthorized();

            var adminId = int.Parse(adminIdClaim);

            // Check if assigned user exists
            var assignedUser = await _context.Users.FindAsync(createInvoiceDto.AssignedUserId);
            if (assignedUser == null)
                return BadRequest("Assigned user not found");

            // Create invoice
            var invoice = new Invoice
            {
                InvoiceNumber = GenerateInvoiceNumber(),
                CustomerName = createInvoiceDto.CustomerName,
                IssueDate = DateTime.UtcNow,
                DueDate = createInvoiceDto.DueDate,
                TotalAmount = 0, // Will calculate below
                Status = "Pending",
                AssignedUserId = createInvoiceDto.AssignedUserId,
                CreatedByAdminId = adminId
            };

            // Calculate total amount and add items
            decimal totalAmount = 0;
            foreach (var itemDto in createInvoiceDto.Items)
            {
                var item = new InvoiceItem
                {
                    Description = itemDto.Description,
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

            // Create notification for assigned user
            var notification = new Notification
            {
                Message = $"New invoice #{invoice.InvoiceNumber} has been assigned to you.",
                CreatedAt = DateTime.UtcNow,
                IsRead = false,
                UserId = invoice.AssignedUserId
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // Send real-time notification via SignalR
            await _hubContext.Clients.User(invoice.AssignedUserId.ToString())
                .SendAsync("ReceiveNotification", $"New invoice #{invoice.InvoiceNumber} has been assigned to you.");

            // Also notify admins
            await _hubContext.Clients.Group("Admins")
                .SendAsync("ReceiveNotification", $"Invoice #{invoice.InvoiceNumber} created for {invoice.CustomerName}");

            var invoiceDto = _mapper.Map<InvoiceDto>(invoice);
            return CreatedAtAction(nameof(GetInvoice), new { id = invoice.Id }, invoiceDto);
        }

        // PUT: api/invoices/5/accept (User accepts invoice)
        [HttpPut("{id}/accept")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> AcceptInvoice(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            var userId = int.Parse(userIdClaim);

            var invoice = await _context.Invoices
                .Include(i => i.CreatedByAdmin)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
                return NotFound();

            // Check if user is assigned to this invoice
            if (invoice.AssignedUserId != userId)
                return Forbid("You are not assigned to this invoice");

            // Check if invoice is already accepted
            if (invoice.Status == "Accepted")
                return BadRequest("Invoice is already accepted");

            // Update invoice status
            invoice.Status = "Accepted";
            invoice.AcceptedDate = DateTime.UtcNow;

            // Create notification for admin
            var adminNotification = new Notification
            {
                Message = $"Invoice #{invoice.InvoiceNumber} has been accepted by {User.Identity?.Name}.",
                CreatedAt = DateTime.UtcNow,
                IsRead = false,
                UserId = invoice.CreatedByAdminId
            };

            _context.Notifications.Add(adminNotification);
            await _context.SaveChangesAsync();

            // Send real-time notification to admin via SignalR
            await _hubContext.Clients.User(invoice.CreatedByAdminId.ToString())
                .SendAsync("ReceiveNotification", $"Invoice #{invoice.InvoiceNumber} has been accepted.");

            // Also notify in admin group
            await _hubContext.Clients.Group("Admins")
                .SendAsync("ReceiveNotification", $"Invoice #{invoice.InvoiceNumber} accepted by user");

            return NoContent();
        }

        // PUT: api/invoices/5/reject (User rejects invoice - optional feature)
        [HttpPut("{id}/reject")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> RejectInvoice(int id, [FromBody] string reason)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            var userId = int.Parse(userIdClaim);

            var invoice = await _context.Invoices
                .Include(i => i.CreatedByAdmin)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
                return NotFound();

            // Check if user is assigned to this invoice
            if (invoice.AssignedUserId != userId)
                return Forbid("You are not assigned to this invoice");

            // Check if invoice is already processed
            if (invoice.Status != "Pending")
                return BadRequest($"Invoice is already {invoice.Status}");

            // Update invoice status
            invoice.Status = "Rejected";

            // Create notification for admin
            var adminNotification = new Notification
            {
                Message = $"Invoice #{invoice.InvoiceNumber} has been rejected by {User.Identity?.Name}. Reason: {reason}",
                CreatedAt = DateTime.UtcNow,
                IsRead = false,
                UserId = invoice.CreatedByAdminId
            };

            _context.Notifications.Add(adminNotification);
            await _context.SaveChangesAsync();

            // Send real-time notification to admin via SignalR
            await _hubContext.Clients.User(invoice.CreatedByAdminId.ToString())
                .SendAsync("ReceiveNotification", $"Invoice #{invoice.InvoiceNumber} has been rejected. Reason: {reason}");

            return NoContent();
        }

        // GET: api/invoices/5/download (Download PDF)
        [HttpGet("{id}/download")]
        public async Task<IActionResult> DownloadInvoicePdf(int id)
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

            // Generate PDF
            var pdfBytes = _pdfService.GenerateInvoicePdf(invoice);

            return File(pdfBytes, "application/pdf", $"Invoice-{invoice.InvoiceNumber}.pdf");
        }

        // GET: api/invoices/notifications (Get user notifications)
        [HttpGet("notifications")]
        public async Task<ActionResult<IEnumerable<Notification>>> GetNotifications()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            var userId = int.Parse(userIdClaim);

            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return Ok(notifications);
        }

        // PUT: api/invoices/notifications/{id}/read (Mark notification as read)
        [HttpPut("notifications/{id}/read")]
        public async Task<IActionResult> MarkNotificationAsRead(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            var userId = int.Parse(userIdClaim);

            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null)
                return NotFound();

            // Check ownership
            if (notification.UserId != userId)
                return Forbid();

            notification.IsRead = true;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/invoices/notifications/{id} (Delete notification)
        [HttpDelete("notifications/{id}")]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            var userId = int.Parse(userIdClaim);

            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null)
                return NotFound();

            // Check ownership
            if (notification.UserId != userId)
                return Forbid();

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/invoices/users (Get users for dropdown - admin only)
        [HttpGet("users")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<object>>> GetUsersForDropdown()
        {
            var users = await _context.Users
                .Where(u => u.Role == "User")
                .Select(u => new
                {
                    u.Id,
                    u.Username,
                    u.Email
                })
                .ToListAsync();

            return Ok(users);
        }

        // Helper method to generate invoice number
        private string GenerateInvoiceNumber()
        {
            var date = DateTime.UtcNow.ToString("yyyyMMdd");
            var random = new Random().Next(1000, 9999);
            return $"INV-{date}-{random}";
        }
    }
}