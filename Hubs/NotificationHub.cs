using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace InvoiceSystem.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
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

        // User joins admin group (called from client when user logs in as admin)
        public async Task JoinAdminGroup()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
        }

        // User leaves admin group
        public async Task LeaveAdminGroup()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Admins");
        }

        // Override to handle user connection
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        // Override to handle user disconnection
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}