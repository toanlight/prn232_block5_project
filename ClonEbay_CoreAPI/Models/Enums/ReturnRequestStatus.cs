namespace ClonEbay_CoreAPI.Models.Enums
{
    /// <summary>
    /// Trạng thái yêu cầu hoàn trả đơn hàng.
    /// Pending  → Chờ xử lý (trạng thái khởi tạo)
    /// Approved → Đã duyệt (Seller/Admin chấp nhận)
    /// Rejected → Đã từ chối (Seller/Admin từ chối)
    /// Cancelled → Đã huỷ (Buyer tự huỷ)
    /// </summary>
    public enum ReturnRequestStatus
    {
        Pending,
        Approved,
        Rejected,
        Cancelled
    }
}
