using System.ComponentModel.DataAnnotations;

namespace CloneEbay_FE.Models
{
    public class ReviewViewModel
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

    public class ProductReviewSummaryViewModel
    {
        public int ProductId { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public List<ReviewViewModel> Reviews { get; set; } = new();
    }

    public class CreateReviewViewModel
    {
        [Required(ErrorMessage = "Mã sản phẩm không được để trống.")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn số sao đánh giá.")]
        [Range(1, 5, ErrorMessage = "Số sao đánh giá phải từ 1 đến 5 sao.")]
        public int Rating { get; set; } = 5;

        [MaxLength(1000, ErrorMessage = "Bình luận không vượt quá 1000 ký tự.")]
        public string? Comment { get; set; }
    }
}
