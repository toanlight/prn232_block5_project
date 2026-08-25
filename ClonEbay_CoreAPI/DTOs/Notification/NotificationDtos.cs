namespace ClonEbay_CoreAPI.DTOs.Notification
{
    /// <summary>
    /// Thông báo Real-time về Đơn hàng (dành cho Seller, Admin, Buyer).
    /// </summary>
    public class OrderNotificationDto
    {
        public int OrderId { get; set; }
        public int BuyerId { get; set; }
        public int SellerId { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Thông báo Real-time về Khuyến mãi / Coupon mới (dành cho Buyer / All users).
    /// </summary>
    public class PromotionNotificationDto
    {
        public int CouponId { get; set; }
        public string Code { get; set; } = string.Empty;
        public decimal DiscountPercent { get; set; }
        public int? ProductId { get; set; }
        public string ProductTitle { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Thông báo Real-time về Đánh giá / Phản hồi sản phẩm (dành cho Seller).
    /// </summary>
    public class FeedbackNotificationDto
    {
        public int ReviewId { get; set; }
        public int ProductId { get; set; }
        public string ProductTitle { get; set; } = string.Empty;
        public int SellerId { get; set; }
        public string ReviewerName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// DTO hiển thị danh sách thông báo người dùng từ CSDL.
    /// </summary>
    public class NotificationDto
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public string? UserRole { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Content { get; set; }
        public string Type { get; set; } = "InApp";
        public string Status { get; set; } = "Sent";
        public bool IsRead { get; set; }
        public string? LinkUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// DTO tạo thông báo mới từ hệ thống hoặc Admin.
    /// </summary>
    public class CreateNotificationDto
    {
        public int? UserId { get; set; }
        public string? UserRole { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Content { get; set; }
        public string Type { get; set; } = "InApp";
        public string? LinkUrl { get; set; }
        public int? CreatedBy { get; set; }
    }

    /// <summary>
    /// DTO số lượng thông báo chưa đọc.
    /// </summary>
    public class UnreadCountDto
    {
        public int UnreadCount { get; set; }
    }
}
