using ClonEbay_CoreAPI.DTOs.Notification;
using ClonEbay_CoreAPI.Hubs;
using ClonEbay_CoreAPI.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace ClonEbay_CoreAPI.Services.Implementations
{
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            IHubContext<NotificationHub> hubContext,
            ILogger<NotificationService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        // ─── 1. Gửi thông báo Đơn hàng ──────────────────────────────────────────
        public async Task SendOrderNotificationAsync(OrderNotificationDto dto)
        {
            _logger.LogInformation("SignalR Push: Đơn hàng #{OrderId} - Message: {Message}", dto.OrderId, dto.Message);

            // Gửi tới Seller sở hữu shop
            if (dto.SellerId > 0)
            {
                await _hubContext.Clients.Group($"User_{dto.SellerId}")
                    .SendAsync("ReceiveOrderNotification", dto);
            }

            // Gửi tới nhóm Role_Seller và Role_Admin
            await _hubContext.Clients.Groups("Role_Seller", "Role_Admin")
                .SendAsync("ReceiveOrderNotification", dto);

            // Gửi tới cá nhân Buyer mua hàng
            if (dto.BuyerId > 0)
            {
                await _hubContext.Clients.Group($"User_{dto.BuyerId}")
                    .SendAsync("ReceiveOrderNotification", dto);
            }
        }

        // ─── 2. Gửi thông báo Khuyến mãi / Coupon mới ─────────────────────────
        public async Task SendPromotionNotificationAsync(PromotionNotificationDto dto)
        {
            _logger.LogInformation("SignalR Push: Khuyến mãi '{Code}' - Message: {Message}", dto.Code, dto.Message);

            // Gửi tới tất cả người dùng thuộc Role_Buyer / User
            await _hubContext.Clients.Group("Role_Buyer")
                .SendAsync("ReceivePromotionNotification", dto);

            // Cũng gửi tới tất cả người dùng đang kết nối (All)
            await _hubContext.Clients.All
                .SendAsync("ReceivePromotionNotification", dto);
        }

        // ─── 3. Gửi thông báo Phản hồi / Đánh giá sản phẩm ──────────────────────
        public async Task SendFeedbackNotificationAsync(FeedbackNotificationDto dto)
        {
            _logger.LogInformation("SignalR Push: Phản hồi/Đánh giá sản phẩm {ProductId} - Seller {SellerId}", dto.ProductId, dto.SellerId);

            // Gửi tới chính Seller sở hữu sản phẩm đó
            if (dto.SellerId > 0)
            {
                await _hubContext.Clients.Group($"User_{dto.SellerId}")
                    .SendAsync("ReceiveFeedbackNotification", dto);
            }

            // Gửi tới nhóm Role_Seller
            await _hubContext.Clients.Group("Role_Seller")
                .SendAsync("ReceiveFeedbackNotification", dto);
        }
    }
}
