using System.ComponentModel.DataAnnotations;

namespace ClonEbay_CoreAPI.DTOs.Coupon
{
    // ─── Response DTOs ────────────────────────────────────────────────────────

    /// <summary>
    /// Thông tin chi tiết một mã giảm giá.
    /// </summary>
    public class CouponDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public decimal DiscountPercent { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int MaxUsage { get; set; }
        public int? ProductId { get; set; }
        public string? ProductTitle { get; set; }
    }

    /// <summary>
    /// Trả về kết quả sau khi áp dụng mã giảm giá.
    /// </summary>
    public class ApplyCouponResponseDto
    {
        public string Code { get; set; } = string.Empty;
        public decimal DiscountPercent { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalPrice { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    // ─── Request DTOs ──────────────────────────────────────────────────────────

    /// <summary>
    /// Body khi Buyer áp dụng mã giảm giá cho sản phẩm.
    /// </summary>
    public class ApplyCouponRequestDto
    {
        [Required(ErrorMessage = "Mã giảm giá không được để trống.")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mã sản phẩm không được để trống.")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Giá gốc sản phẩm không được để trống.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Giá sản phẩm phải lớn hơn 0.")]
        public decimal OriginalPrice { get; set; }
    }

    /// <summary>
    /// Body khi Seller/Admin tạo mã giảm giá mới.
    /// </summary>
    public class CreateCouponDto
    {
        [Required(ErrorMessage = "Mã giảm giá không được để trống.")]
        [MaxLength(50, ErrorMessage = "Mã giảm giá không được vượt quá 50 ký tự.")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phần trăm giảm giá không được để trống.")]
        [Range(1, 100, ErrorMessage = "Phần trăm giảm giá phải từ 1% đến 100%.")]
        public decimal DiscountPercent { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [Required(ErrorMessage = "Số lần sử dụng tối đa không được để trống.")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lần sử dụng tối đa phải ít nhất là 1.")]
        public int MaxUsage { get; set; }

        [Required(ErrorMessage = "Mã sản phẩm không được để trống.")]
        public int ProductId { get; set; }
    }

    /// <summary>
    /// Body khi Seller/Admin cập nhật mã giảm giá.
    /// </summary>
    public class UpdateCouponDto
    {
        [Required(ErrorMessage = "Mã giảm giá không được để trống.")]
        [MaxLength(50, ErrorMessage = "Mã giảm giá không được vượt quá 50 ký tự.")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phần trăm giảm giá không được để trống.")]
        [Range(1, 100, ErrorMessage = "Phần trăm giảm giá phải từ 1% đến 100%.")]
        public decimal DiscountPercent { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [Required(ErrorMessage = "Số lần sử dụng tối đa không được để trống.")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lần sử dụng tối đa phải ít nhất là 1.")]
        public int MaxUsage { get; set; }

        [Required(ErrorMessage = "Mã sản phẩm không được để trống.")]
        public int ProductId { get; set; }
    }
}
