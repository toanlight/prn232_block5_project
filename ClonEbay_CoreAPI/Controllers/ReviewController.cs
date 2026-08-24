using System.Security.Claims;
using ClonEbay_CoreAPI.Common.Models;
using ClonEbay_CoreAPI.DTOs.Review;
using ClonEbay_CoreAPI.Exceptions;
using ClonEbay_CoreAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClonEbay_CoreAPI.Controllers
{
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        // ─── GET /api/products/{productId}/reviews ──────────────────────────
        /// <summary>Xem tất cả đánh giá của 1 sản phẩm kèm điểm số trung bình (Công khai).</summary>
        [HttpGet("api/products/{productId:int}/reviews")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<ProductReviewSummaryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProductReviews(int productId)
        {
            return Ok(await _reviewService.GetProductReviewsAsync(productId));
        }

        // ─── GET /api/reviews/my-reviews ─────────────────────────────────────
        /// <summary>Người dùng đã đăng nhập xem lại tất cả các đánh giá mình đã viết.</summary>
        [HttpGet("api/reviews/my-reviews")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<List<ReviewDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyReviews()
        {
            return Ok(await _reviewService.GetMyReviewsAsync(GetCurrentUserId()));
        }

        // ─── POST /api/reviews ────────────────────────────────────────────────
        /// <summary>Người mua gửi đánh giá sản phẩm mới (yêu cầu đã mua và đơn hàng Delivered).</summary>
        [HttpPost("api/reviews")]
        [Authorize(Roles = "Buyer")]
        [ProducesResponseType(typeof(ApiResponse<ReviewDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Create([FromBody] CreateReviewDto dto)
        {
            return Ok(await _reviewService.CreateReviewAsync(GetCurrentUserId(), dto));
        }

        // ─── PUT /api/reviews/{id} ─────────────────────────────────────────────
        /// <summary>Người mua chỉnh sửa lại đánh giá của chính mình.</summary>
        [HttpPut("api/reviews/{id:int}")]
        [Authorize(Roles = "Buyer")]
        [ProducesResponseType(typeof(ApiResponse<ReviewDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateReviewDto dto)
        {
            return Ok(await _reviewService.UpdateReviewAsync(GetCurrentUserId(), id, dto));
        }

        // ─── DELETE /api/reviews/{id} ──────────────────────────────────────────
        /// <summary>Xóa đánh giá (chủ sở hữu đánh giá hoặc Admin).</summary>
        [HttpDelete("api/reviews/{id:int}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            return Ok(await _reviewService.DeleteReviewAsync(GetCurrentUserId(), GetCurrentUserRole(), id));
        }

        // ─── Private helpers ────────────────────────────────────────────────────
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedException("Vui lòng đăng nhập để thực hiện chức năng này.");
            }
            return userId;
        }

        private string GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        }
    }
}
