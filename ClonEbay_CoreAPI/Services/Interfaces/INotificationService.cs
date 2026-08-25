using ClonEbay_CoreAPI.Common.Models;
using ClonEbay_CoreAPI.DTOs.Notification;

namespace ClonEbay_CoreAPI.Services.Interfaces
{
    public interface INotificationService
    {
        Task<ApiResponse<List<NotificationDto>>> GetUserNotificationsAsync(int userId, string userRole);
        Task<ApiResponse<UnreadCountDto>> GetUnreadCountAsync(int userId, string userRole);
        Task<ApiResponse<bool>> MarkAsReadAsync(int notificationId, int userId);
        Task<ApiResponse<bool>> MarkAllAsReadAsync(int userId, string userRole);
        Task CreateNotificationAsync(CreateNotificationDto dto);

        Task SendOrderNotificationAsync(OrderNotificationDto dto);
        Task SendPromotionNotificationAsync(PromotionNotificationDto dto);
        Task SendFeedbackNotificationAsync(FeedbackNotificationDto dto);
    }
}
