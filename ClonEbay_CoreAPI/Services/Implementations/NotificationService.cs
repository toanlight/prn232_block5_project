using ClonEbay_CoreAPI.Common.Models;
using ClonEbay_CoreAPI.DTOs.Notification;
using ClonEbay_CoreAPI.Hubs;
using ClonEbay_CoreAPI.Models;
using ClonEbay_CoreAPI.Repositories.Interfaces;
using ClonEbay_CoreAPI.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ClonEbay_CoreAPI.Services.Implementations
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepo;
        private readonly CloneEbayDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            INotificationRepository notificationRepo,
            CloneEbayDbContext context,
            IHubContext<NotificationHub> hubContext,
            ILogger<NotificationService> logger)
        {
            _notificationRepo = notificationRepo;
            _context = context;
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task<ApiResponse<List<NotificationDto>>> GetUserNotificationsAsync(int userId, string userRole)
        {
            var list = await _notificationRepo.GetUserNotificationsAsync(userId, userRole);

            var readNotificationIds = await _context.UserNotificationReads
                .Where(r => r.UserId == userId)
                .Select(r => r.NotificationId)
                .ToListAsync();

            var dtos = list.Select(n => new NotificationDto
            {
                Id = n.Id,
                UserId = n.UserId,
                UserRole = n.UserRole,
                Title = n.Title,
                Content = n.Content,
                Type = n.Type,
                Status = n.Status,
                IsRead = n.UserId == userId ? n.IsRead : readNotificationIds.Contains(n.Id),
                LinkUrl = n.LinkUrl,
                CreatedAt = n.CreatedAt
            }).ToList();

            return ApiResponse<List<NotificationDto>>.Ok(dtos);
        }

        public async Task<ApiResponse<UnreadCountDto>> GetUnreadCountAsync(int userId, string userRole)
        {
            var count = await _notificationRepo.GetUnreadCountAsync(userId, userRole);
            return ApiResponse<UnreadCountDto>.Ok(new UnreadCountDto { UnreadCount = count });
        }

        public async Task<ApiResponse<bool>> MarkAsReadAsync(int notificationId, int userId)
        {
            await _notificationRepo.MarkAsReadAsync(notificationId, userId);
            return ApiResponse<bool>.Ok(true, "Đã đánh dấu thông báo là đã đọc.");
        }

        public async Task<ApiResponse<bool>> MarkAllAsReadAsync(int userId, string userRole)
        {
            await _notificationRepo.MarkAllAsReadAsync(userId, userRole);
            return ApiResponse<bool>.Ok(true, "Đã đánh dấu tất cả thông báo là đã đọc.");
        }

        public async Task CreateNotificationAsync(CreateNotificationDto dto)
        {
            var notif = new Notification
            {
                UserId = dto.UserId,
                UserRole = dto.UserRole,
                Title = dto.Title,
                Content = dto.Content,
                Type = dto.Type,
                LinkUrl = dto.LinkUrl,
                CreatedBy = dto.CreatedBy,
                Status = "Sent",
                SentAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            await _notificationRepo.AddAsync(notif);
            await _notificationRepo.SaveChangesAsync();
        }

        // ─── 1. Gửi thông báo Đơn hàng & Ghi CSDL ──────────────────────────────────
        public async Task SendOrderNotificationAsync(OrderNotificationDto dto)
        {
            _logger.LogInformation("SignalR Push: Đơn hàng #{OrderId} - Message: {Message}", dto.OrderId, dto.Message);

            // Ghi nhận DB cho Buyer nếu có BuyerId
            if (dto.BuyerId > 0)
            {
                await CreateNotificationAsync(new CreateNotificationDto
                {
                    UserId = dto.BuyerId,
                    UserRole = "Buyer",
                    Title = $"Thông báo Đơn hàng #{dto.OrderId}",
                    Content = dto.Message,
                    Type = "Order",
                    LinkUrl = $"/Profile/Orders"
                });
            }

            // Ghi nhận DB cho Seller nếu có SellerId
            if (dto.SellerId > 0 && dto.SellerId != dto.BuyerId)
            {
                await CreateNotificationAsync(new CreateNotificationDto
                {
                    UserId = dto.SellerId,
                    UserRole = "Seller",
                    Title = $"Thông báo Đơn hàng #{dto.OrderId}",
                    Content = dto.Message,
                    Type = "Order",
                    LinkUrl = $"/ReturnRequest"
                });
            }

            // Real-time Push qua SignalR
            if (dto.SellerId > 0)
            {
                await _hubContext.Clients.Group($"User_{dto.SellerId}")
                    .SendAsync("ReceiveOrderNotification", dto);
            }

            await _hubContext.Clients.Groups("Role_Seller", "Role_Admin")
                .SendAsync("ReceiveOrderNotification", dto);

            if (dto.BuyerId > 0)
            {
                await _hubContext.Clients.Group($"User_{dto.BuyerId}")
                    .SendAsync("ReceiveOrderNotification", dto);
            }
        }

        // ─── 2. Gửi thông báo Khuyến mãi / Coupon & Ghi CSDL ─────────────────────
        public async Task SendPromotionNotificationAsync(PromotionNotificationDto dto)
        {
            _logger.LogInformation("SignalR Push: Khuyến mãi '{Code}' - Message: {Message}", dto.Code, dto.Message);

            // Ghi nhận DB thông báo Broadcast Khuyến mãi cho Buyer
            await CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = null,
                UserRole = "Buyer",
                Title = $"Khuyến mãi Mới: mã '{dto.Code}'",
                Content = dto.Message,
                Type = "Promotion",
                LinkUrl = dto.ProductId.HasValue ? $"/Products/Details/{dto.ProductId}" : "/"
            });

            await _hubContext.Clients.Group("Role_Buyer")
                .SendAsync("ReceivePromotionNotification", dto);

            await _hubContext.Clients.All
                .SendAsync("ReceivePromotionNotification", dto);
        }

        // ─── 3. Gửi thông báo Phản hồi / Đánh giá & Ghi CSDL ─────────────────────
        public async Task SendFeedbackNotificationAsync(FeedbackNotificationDto dto)
        {
            _logger.LogInformation("SignalR Push: Phản hồi/Đánh giá sản phẩm {ProductId} - Seller {SellerId}", dto.ProductId, dto.SellerId);

            if (dto.SellerId > 0)
            {
                await CreateNotificationAsync(new CreateNotificationDto
                {
                    UserId = dto.SellerId,
                    UserRole = "Seller",
                    Title = $"Đánh giá mới cho sản phẩm: {dto.ProductTitle}",
                    Content = dto.Message,
                    Type = "Feedback",
                    LinkUrl = $"/Products/Details/{dto.ProductId}"
                });

                await _hubContext.Clients.Group($"User_{dto.SellerId}")
                    .SendAsync("ReceiveFeedbackNotification", dto);
            }

            await _hubContext.Clients.Group("Role_Seller")
                .SendAsync("ReceiveFeedbackNotification", dto);
        }
    }
}
