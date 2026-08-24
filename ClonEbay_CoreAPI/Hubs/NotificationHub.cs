using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ClonEbay_CoreAPI.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> _logger;

        public NotificationHub(ILogger<NotificationHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = Context.User?.FindFirst(ClaimTypes.Role)?.Value ?? "User";

            if (!string.IsNullOrEmpty(userId))
            {
                // Thêm vào nhóm riêng của cá nhân User
                await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
                _logger.LogInformation("SignalR: Connection {ConnectionId} đã tham gia nhóm User_{UserId}", Context.ConnectionId, userId);
            }

            if (!string.IsNullOrEmpty(role))
            {
                // Thêm vào nhóm theo Role (Role_Buyer, Role_Seller, Role_Admin)
                await Groups.AddToGroupAsync(Context.ConnectionId, $"Role_{role}");
                
                // Đồng bộ tên role tương thích (User & Buyer)
                if (string.Equals(role, "User", StringComparison.OrdinalIgnoreCase))
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, "Role_Buyer");
                }

                _logger.LogInformation("SignalR: Connection {ConnectionId} đã tham gia nhóm Role_{Role}", Context.ConnectionId, role);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation("SignalR: Connection {ConnectionId} đã ngắt kết nối.", Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }
    }
}
