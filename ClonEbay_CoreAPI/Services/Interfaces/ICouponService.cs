using ClonEbay_CoreAPI.Common.Models;
using ClonEbay_CoreAPI.DTOs.Coupon;

namespace ClonEbay_CoreAPI.Services.Interfaces
{
    public interface ICouponService
    {
        /// <summary>Xem tất cả các mã giảm giá còn hiệu lực.</summary>
        Task<ApiResponse<List<CouponDto>>> GetActiveCouponsAsync();

        /// <summary>Buyer áp dụng mã giảm giá và nhận kết quả tính tiền giảm (MaxUsage giảm 1).</summary>
        Task<ApiResponse<ApplyCouponResponseDto>> ApplyCouponAsync(ApplyCouponRequestDto dto);

        /// <summary>Seller/Admin tạo mã giảm giá mới.</summary>
        Task<ApiResponse<CouponDto>> CreateCouponAsync(CreateCouponDto dto);

        /// <summary>Seller/Admin cập nhật mã giảm giá.</summary>
        Task<ApiResponse<CouponDto>> UpdateCouponAsync(int id, UpdateCouponDto dto);

        /// <summary>Seller/Admin xóa mã giảm giá.</summary>
        Task<ApiResponse<bool>> DeleteCouponAsync(int id);
    }
}
