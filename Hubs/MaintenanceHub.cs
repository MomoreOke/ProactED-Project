using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace FEENALOoFINALE.Hubs
{
    [Authorize]
    public class MaintenanceHub : Hub
    {
        /// <summary>
        /// Join a specific group for targeted notifications
        /// </summary>
        /// <param name="groupName">Group name (e.g., "Dashboard", "Alerts", "Maintenance")</param>
        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            
            // Notify other clients in the group that someone joined
            await Clients.Group(groupName).SendAsync("UserJoined", Context.User?.Identity?.Name);
        }

        /// <summary>
        /// Leave a specific group
        /// </summary>
        /// <param name="groupName">Group name to leave</param>
        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName).SendAsync("UserLeft", Context.User?.Identity?.Name);
        }

        /// <summary>
        /// Send a message to all connected clients
        /// </summary>
        /// <param name="message">Message to broadcast</param>
        public async Task SendMessageToAll(string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", Context.User?.Identity?.Name, message);
        }

        /// <summary>
        /// Send a notification to a specific group
        /// </summary>
        /// <param name="groupName">Target group</param>
        /// <param name="type">Notification type (success, warning, error, info)</param>
        /// <param name="title">Notification title</param>
        /// <param name="message">Notification message</param>
        public async Task SendNotificationToGroup(string groupName, string type, string title, string message)
        {
            await Clients.Group(groupName).SendAsync("ReceiveNotification", new
            {
                Type = type,
                Title = title,
                Message = message,
                Timestamp = DateTime.Now,
                User = Context.User?.Identity?.Name
            });
        }

        /// <summary>
        /// Update dashboard metrics in real-time
        /// </summary>
        /// <param name="metrics">Dashboard metrics object</param>
        public async Task UpdateDashboardMetrics(object metrics)
        {
            await Clients.Group("Dashboard").SendAsync("DashboardUpdate", metrics);
        }

        /// <summary>
        /// Broadcast new alert to all users
        /// </summary>
        /// <param name="alert">Alert object</param>
        public async Task BroadcastAlert(object alert)
        {
            await Clients.All.SendAsync("NewAlert", alert);
        }

        /// <summary>
        /// Handle connection events
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            // Auto-join to common groups
            await Groups.AddToGroupAsync(Context.ConnectionId, "Notifications");
            
            // Log connection
            Console.WriteLine($"User {Context.User?.Identity?.Name} connected to SignalR hub");
            
            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Handle disconnection events
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // Log disconnection
            Console.WriteLine($"User {Context.User?.Identity?.Name} disconnected from SignalR hub");
            
            await base.OnDisconnectedAsync(exception);
        }
    }
}
