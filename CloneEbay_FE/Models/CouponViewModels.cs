using System.ComponentModel.DataAnnotations;

namespace CloneEbay_FE.Models
{
    public class CouponViewModel
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

    public class ApplyCouponViewModel
    {
        [Required(ErrorMessage = "Mã giảm giá không được để trống.")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mã sản phẩm không được để trống.")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Giá gốc sản phẩm không được để trống.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Giá trị phải lớn hơn 0.")]
        public decimal OriginalPrice { get; set; }
    }

    public class ApplyCouponResultViewModel
    {
        public string Code { get; set; } = string.Empty;
        public decimal DiscountPercent { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalPrice { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class CreateCouponViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập mã giảm giá.")]
        [MaxLength(50, ErrorMessage = "Mã không vượt quá 50 ký tự.")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập phần trăm giảm.")]
        [Range(1, 100, ErrorMessage = "Phần trăm phải từ 1 đến 100.")]
        public decimal DiscountPercent { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số lần dùng tối đa.")]
        [Range(1, int.MaxValue, ErrorMessage = "Tối thiểu 1 lần.")]
        public int MaxUsage { get; set; } = 10;

        [Required(ErrorMessage = "Vui lòng chọn sản phẩm áp dụng.")]
        public int ProductId { get; set; }
    }
}
