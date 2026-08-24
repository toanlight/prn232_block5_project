using ClonEbay_CoreAPI.Common.Models;
using ClonEbay_CoreAPI.DTOs.Coupon;
using ClonEbay_CoreAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClonEbay_CoreAPI.Controllers
{
    [ApiController]
    [Route("api/coupons")]
    public class CouponController : ControllerBase
    {
        private readonly ICouponService _couponService;

        public CouponController(ICouponService couponService)
        {
            _couponService = couponService;
        }

        // ─── GET /api/coupons ──────────────────────────────────────────────────
        /// <summary>Xem danh sách tất cả mã giảm giá đang còn hiệu lực (Công khai).</summary>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<List<CouponDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetActiveCoupons()
        {
            return Ok(await _couponService.GetActiveCouponsAsync());
        }

        // ─── POST /api/coupons/apply ───────────────────────────────────────────
        /// <summary>Buyer áp dụng mã giảm giá để tính toán số tiền giảm (MaxUsage giảm 1).</summary>
        [HttpPost("apply")]
        [Authorize(Roles = "Buyer,BUYER,buyer,Seller,Admin,SELLER,ADMIN")]
        [ProducesResponseType(typeof(ApiResponse<ApplyCouponResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ApplyCoupon([FromBody] ApplyCouponRequestDto dto)
        {
            return Ok(await _couponService.ApplyCouponAsync(dto));
        }

        // ─── POST /api/coupons ─────────────────────────────────────────────────
        /// <summary>Seller hoặc Admin tạo mã giảm giá mới cho sản phẩm.</summary>
        [HttpPost]
        [Authorize(Roles = "Seller,Admin,SELLER,ADMIN,seller,admin")]
        [ProducesResponseType(typeof(ApiResponse<CouponDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Create([FromBody] CreateCouponDto dto)
        {
            return Ok(await _couponService.CreateCouponAsync(dto));
        }

        // ─── PUT /api/coupons/{id} ──────────────────────────────────────────────
        /// <summary>Seller hoặc Admin cập nhật thông tin mã giảm giá.</summary>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Seller,Admin,SELLER,ADMIN,seller,admin")]
        [ProducesResponseType(typeof(ApiResponse<CouponDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCouponDto dto)
        {
            return Ok(await _couponService.UpdateCouponAsync(id, dto));
        }

        // ─── DELETE /api/coupons/{id} ───────────────────────────────────────────
        /// <summary>Seller hoặc Admin xóa mã giảm giá.</summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Seller,Admin,SELLER,ADMIN,seller,admin")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            return Ok(await _couponService.DeleteCouponAsync(id));
        }
    }
}
