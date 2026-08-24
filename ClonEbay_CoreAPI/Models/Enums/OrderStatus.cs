namespace ClonEbay_CoreAPI.Models.Enums
{
    /// <summary>
    /// Trạng thái của đơn hàng trong hệ thống (Chuẩn eBay):
    /// Pending   → Đơn hàng mới khởi tạo / Chờ thanh toán PayPal
    /// Confirmed → Đã xác nhận (Đặt hàng thành công COD hoặc đã thanh toán PayPal)
    /// Shipping  → Đang vận chuyển
    /// Delivered → Đã nhận hàng thành công
    /// Cancelled → Đã huỷ đơn hàng
    /// Returned  → Đã hoàn trả đơn hàng
    /// </summary>
    public enum OrderStatus
    {
        Pending,
        Confirmed,
        Shipping,
        Delivered,
        Cancelled,
        Returned
    }
}
