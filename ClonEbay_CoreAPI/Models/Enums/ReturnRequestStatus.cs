namespace ClonEbay_CoreAPI.Models.Enums
{
    /// <summary>
    /// Trạng thái tiến trình hoàn trả sản phẩm (eBay Return State Machine).
    /// Requested  -> Yêu cầu hoàn trả vừa khởi tạo (Chờ Seller phản hồi)
    /// Pending    -> Trạng thái chờ xử lý ban đầu
    /// Approved   -> Seller đã duyệt yêu cầu (Chờ Buyer gửi trả hàng)
    /// Returning  -> Buyer đã gửi hàng trả lại (Đã cập nhật mã vận chuyển)
    /// Returned   -> Seller đã nhận được sản phẩm trả về
    /// Refunded   -> Đã hoàn tiền thành công (Hoàn tiền toàn phần / một phần)
    /// Rejected   -> Seller từ chối yêu cầu hoàn trả
    /// Cancelled  -> Buyer hủy yêu cầu hoàn trả
    /// Escalated  -> Buyer yêu cầu Admin can thiệp (Ask eBay to step in)
    /// </summary>
    public enum ReturnRequestStatus
    {
        Requested,
        Pending,
        Approved,
        Returning,
        Returned,
        Refunded,
        Rejected,
        Cancelled,
        Escalated
    }

    /// <summary>
    /// Lý do hoàn trả sản phẩm chuẩn eBay.
    /// </summary>
    public enum ReturnReason
    {
        Damaged,         // Sản phẩm bị hư hỏng / bể vỡ
        Defective,       // Sản phẩm bị lỗi kỹ thuật / không hoạt động
        WrongItem,       // Giao sai sản phẩm
        NotAsDescribed,  // Không đúng mô tả / khác với hình ảnh
        ChangedMind      // Thay đổi nhu cầu / Không vừa kích thước
    }
}
