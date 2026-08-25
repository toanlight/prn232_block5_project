using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace CloneEbay_FE.Models
{
    public class ReturnEvidenceViewModel
    {
        public int Id { get; set; }
        public string FileUrl { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
    }

    public class ReturnRequestViewModel
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
        public List<ReturnEvidenceViewModel> Evidences { get; set; } = new();
    }

    public class CreateReturnRequestViewModel
    {
        [Required(ErrorMessage = "Mã đơn hàng không được để trống.")]
        public int OrderId { get; set; }

        public bool ReturnEntireOrder { get; set; } = false;

        public List<int> SelectedOrderItemIds { get; set; } = new();

        public int? OrderItemId { get; set; }

        public int? ProductId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn lý do hoàn trả.")]
        public string SelectedReasonCategory { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập chi tiết lý do hoàn trả.")]
        [MaxLength(1000, ErrorMessage = "Lý do không vượt quá 1000 ký tự.")]
        public string DetailedReason { get; set; } = string.Empty;

        public List<IFormFile>? EvidenceFiles { get; set; }
    }

    public class UpdateReturnTrackingViewModel
    {
        [Required(ErrorMessage = "Mã vận chuyển không được để trống.")]
        public string TrackingNumber { get; set; } = string.Empty;
    }

    public class EscalateReturnViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập lý do cần Admin can thiệp.")]
        public string Reason { get; set; } = string.Empty;
    }

    public class ProcessReturnRefundViewModel
    {
        [Required(ErrorMessage = "Trạng thái không được để trống.")]
        public string Status { get; set; } = string.Empty;
        public string? RefundType { get; set; } // Full / Partial
        public decimal? RefundAmount { get; set; }
        public string? AdminNotes { get; set; }
    }
}
