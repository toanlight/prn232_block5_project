using ClonEbay_CoreAPI.Common.Models;
using ClonEbay_CoreAPI.DTOs.Review;

namespace ClonEbay_CoreAPI.Services.Interfaces
{
    public interface IReviewService
    {
        /// <summary>Xem tất cả đánh giá của 1 sản phẩm kèm điểm trung bình.</summary>
        Task<ApiResponse<ProductReviewSummaryDto>> GetProductReviewsAsync(int productId);

        /// <summary>Buyer xem lại tất cả đánh giá do chính mình đã gửi.</summary>
        Task<ApiResponse<List<ReviewDto>>> GetMyReviewsAsync(int userId);

        /// <summary>Gửi đánh giá mới cho 1 sản phẩm (yêu cầu đã mua và Delivered).</summary>
        Task<ApiResponse<ReviewDto>> CreateReviewAsync(int userId, CreateReviewDto dto);

        /// <summary>Cập nhật đánh giá của chính mình.</summary>
        Task<ApiResponse<ReviewDto>> UpdateReviewAsync(int userId, int reviewId, UpdateReviewDto dto);

        /// <summary>Xóa đánh giá (chủ sở hữu hoặc Admin).</summary>
        Task<ApiResponse<bool>> DeleteReviewAsync(int userId, string role, int reviewId);
    }
}
