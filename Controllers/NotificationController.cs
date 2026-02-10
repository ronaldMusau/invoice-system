using InvoiceSystem.Data;
using InvoiceSystem.Hubs;
using InvoiceSystem.Models;
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
    public class NotificationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationController(ApplicationDbContext context, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // GET: api/notification
        [HttpGet]
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

        // PUT: api/notification/5/read
        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            var userId = int.Parse(userIdClaim);

            var notification = await _context.Notifications.FindAsync(id);

            if (notification == null)
                return NotFound();

            if (notification.UserId != userId)
                return Forbid();

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();

                // Send notification update to the user
                try
                {
                    await _hubContext.Clients.User(userId.ToString())
                        .SendAsync("NotificationUpdated", new
                        {
                            notificationId = id,
                            isRead = true,
                            unreadCount = await _context.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead)
                        });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"SignalR notification update failed: {ex.Message}");
                }
            }

            return Ok(new
            {
                message = "Notification marked as read",
                notificationId = id,
                isRead = true
            });
        }

        // PUT: api/notification/mark-all-read
        [HttpPut("mark-all-read")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            var userId = int.Parse(userIdClaim);

            var unreadNotifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();

            // Send notification update to the user
            try
            {
                await _hubContext.Clients.User(userId.ToString())
                    .SendAsync("AllNotificationsRead", new
                    {
                        unreadCount = 0
                    });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SignalR all read notification failed: {ex.Message}");
            }

            return Ok(new
            {
                message = "All notifications marked as read",
                markedCount = unreadNotifications.Count
            });
        }

        // DELETE: api/notification/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            var userId = int.Parse(userIdClaim);

            var notification = await _context.Notifications.FindAsync(id);

            if (notification == null)
                return NotFound();

            if (notification.UserId != userId)
                return Forbid();

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();

            // Send notification update to the user
            try
            {
                await _hubContext.Clients.User(userId.ToString())
                    .SendAsync("NotificationDeleted", new
                    {
                        notificationId = id,
                        unreadCount = await _context.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead)
                    });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SignalR delete notification failed: {ex.Message}");
            }

            return Ok(new
            {
                message = "Notification deleted successfully",
                notificationId = id
            });
        }

        // GET: api/notification/unread-count
        [HttpGet("unread-count")]
        public async Task<ActionResult<int>> GetUnreadCount()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            var userId = int.Parse(userIdClaim);

            var unreadCount = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .CountAsync();

            return Ok(unreadCount);
        }
    }
}