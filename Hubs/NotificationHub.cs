using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace InvoiceSystem.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> _logger;

        public NotificationHub(ILogger<NotificationHub> logger)
        {
            _logger = logger;
        }

        // Send notification to specific user
        public async Task SendNotificationToUser(string userId, string message)
        {
            await Clients.User(userId).SendAsync("ReceiveNotification", message);
        }

        // Send notification to admin users
        public async Task SendNotificationToAdmins(string message)
        {
            await Clients.Group("Admins").SendAsync("ReceiveNotification", message);
        }

        // Send notification update (when marked as read, deleted, etc.)
        public async Task SendNotificationUpdate(string userId, object updateData)
        {
            await Clients.User(userId).SendAsync("NotificationUpdated", updateData);
        }

        // User joins admin group (called from client when user logs in as admin)
        public async Task JoinAdminGroup()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
                _logger.LogInformation($"User {userId} joined Admin group");
            }
        }

        // User leaves admin group
        public async Task LeaveAdminGroup()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Admins");
        }

        // Override to handle user connection
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
            var role = Context.User?.FindFirst(ClaimTypes.Role)?.Value;

            _logger.LogInformation($"User connected: {username} (ID: {userId}, Role: {role})");

            await base.OnConnectedAsync();
        }

        // Override to handle user disconnection
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = Context.User?.FindFirst(ClaimTypes.Name)?.Value;

            if (exception != null)
            {
                _logger.LogWarning($"User disconnected with error: {username} (ID: {userId}) - {exception.Message}");
            }
            else
            {
                _logger.LogInformation($"User disconnected: {username} (ID: {userId})");
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}