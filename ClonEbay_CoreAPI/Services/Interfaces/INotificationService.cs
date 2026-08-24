using ClonEbay_CoreAPI.DTOs.Notification;

namespace ClonEbay_CoreAPI.Services.Interfaces
{
    public interface INotificationService
    {
        /// <summary>Gửi thông báo Đơn hàng tới Seller (chủ shop) và Admin / Buyer.</summary>
        Task SendOrderNotificationAsync(OrderNotificationDto dto);

        /// <summary>Gửi thông báo Khuyến mãi / Coupon mới tới các Buyer.</summary>
        Task SendPromotionNotificationAsync(PromotionNotificationDto dto);

        /// <summary>Gửi thông báo Đánh giá / Phản hồi tới Seller sở hữu sản phẩm.</summary>
        Task SendFeedbackNotificationAsync(FeedbackNotificationDto dto);
    }
}
