using System.ComponentModel.DataAnnotations;

namespace ClonEbay_CoreAPI.DTOs.Review
{
    // ─── Response DTOs ────────────────────────────────────────────────────────

    /// <summary>
    /// Thông tin chi tiết một đánh giá sản phẩm.
    /// </summary>
    public class ReviewDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductTitle { get; set; } = string.Empty;
        public int ReviewerId { get; set; }
        public string ReviewerName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Tổng hợp đánh giá của một sản phẩm (điểm trung bình + danh sách reviews).
    /// </summary>
    public class ProductReviewSummaryDto
    {
        public int ProductId { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public List<ReviewDto> Reviews { get; set; } = new();
    }

    // ─── Request DTOs ──────────────────────────────────────────────────────────

    /// <summary>
    /// Body khi Buyer gửi đánh giá mới cho sản phẩm.
    /// </summary>
    public class CreateReviewDto
    {
        [Required(ErrorMessage = "Mã sản phẩm không được để trống.")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Số sao đánh giá không được để trống.")]
        [Range(1, 5, ErrorMessage = "Số sao đánh giá phải từ 1 đến 5 sao.")]
        public int Rating { get; set; }

        [MaxLength(1000, ErrorMessage = "Bình luận không được vượt quá 1000 ký tự.")]
        public string? Comment { get; set; }
    }

    /// <summary>
    /// Body khi Buyer cập nhật đánh giá của mình.
    /// </summary>
    public class UpdateReviewDto
    {
        [Required(ErrorMessage = "Số sao đánh giá không được để trống.")]
        [Range(1, 5, ErrorMessage = "Số sao đánh giá phải từ 1 đến 5 sao.")]
        public int Rating { get; set; }

        [MaxLength(1000, ErrorMessage = "Bình luận không được vượt quá 1000 ký tự.")]
        public string? Comment { get; set; }
    }
}
