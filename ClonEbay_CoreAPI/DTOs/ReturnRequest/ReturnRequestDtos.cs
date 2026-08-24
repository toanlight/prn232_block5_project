using System.ComponentModel.DataAnnotations;

namespace ClonEbay_CoreAPI.DTOs.ReturnRequest
{
    // ─── Response DTO ─────────────────────────────────────────────────────────

    /// <summary>
    /// Dữ liệu trả về cho client khi xem yêu cầu hoàn trả.
    /// </summary>
    public class ReturnRequestDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    // ─── Request DTOs ──────────────────────────────────────────────────────────

    /// <summary>
    /// Body khi Buyer gửi yêu cầu hoàn trả mới.
    /// </summary>
    public class CreateReturnRequestDto
    {
        [Required(ErrorMessage = "Mã đơn hàng không được để trống.")]
        public int OrderId { get; set; }

        [Required(ErrorMessage = "Lý do hoàn trả không được để trống.")]
        [MaxLength(1000, ErrorMessage = "Lý do không được vượt quá 1000 ký tự.")]
        public string Reason { get; set; } = string.Empty;
    }

    /// <summary>
    /// Body khi Seller/Admin duyệt hoặc từ chối yêu cầu hoàn trả.
    /// </summary>
    public class UpdateReturnRequestStatusDto
    {
        [Required(ErrorMessage = "Trạng thái không được để trống.")]
        [RegularExpression("^(Approved|Rejected)$",
            ErrorMessage = "Trạng thái chỉ được là 'Approved' hoặc 'Rejected'.")]
        public string Status { get; set; } = string.Empty;
    }
}
