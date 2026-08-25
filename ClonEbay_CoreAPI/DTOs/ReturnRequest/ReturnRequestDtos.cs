using System.ComponentModel.DataAnnotations;

namespace ClonEbay_CoreAPI.DTOs.ReturnRequest
{
    public class ReturnEvidenceDto
    {
        public int Id { get; set; }
        public string FileUrl { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
    }

    public class ReturnRequestDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int? OrderItemId { get; set; }
        public int? ProductId { get; set; }
        public string ProductTitle { get; set; } = string.Empty;
        public string? ProductImageUrl { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public int UserId { get; set; }
        public string BuyerName { get; set; } = string.Empty;
        public int? SellerId { get; set; }
        public string SellerName { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal? RefundAmount { get; set; }
        public string? RefundType { get; set; }
        public string? TrackingNumber { get; set; }
        public bool IsEscalated { get; set; }
        public string? EscalationReason { get; set; }
        public string? AdminNotes { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<ReturnEvidenceDto> Evidences { get; set; } = new();
    }

    public class CreateReturnRequestDto
    {
        [Required(ErrorMessage = "Mã đơn hàng không được để trống.")]
        public int OrderId { get; set; }

        public bool ReturnEntireOrder { get; set; } = false;

        public List<int> SelectedOrderItemIds { get; set; } = new();

        public int? OrderItemId { get; set; }

        public int? ProductId { get; set; }

        [Required(ErrorMessage = "Lý do hoàn trả không được để trống.")]
        [MaxLength(1000, ErrorMessage = "Lý do không được vượt quá 1000 ký tự.")]
        public string Reason { get; set; } = string.Empty;

        public List<string> Evidences { get; set; } = new();
    }

    public class UpdateReturnRequestStatusDto
    {
        [Required(ErrorMessage = "Trạng thái không được để trống.")]
        public string Status { get; set; } = string.Empty;
        public string? RefundType { get; set; } // Full / Partial
        public decimal? RefundAmount { get; set; }
        public string? AdminNotes { get; set; }
    }

    public class UpdateReturnTrackingDto
    {
        [Required(ErrorMessage = "Mã vận chuyển không được để trống.")]
        public string TrackingNumber { get; set; } = string.Empty;
    }

    public class EscalateReturnDto
    {
        [Required(ErrorMessage = "Lý do yêu cầu Admin can thiệp không được để trống.")]
        public string Reason { get; set; } = string.Empty;
    }
}
