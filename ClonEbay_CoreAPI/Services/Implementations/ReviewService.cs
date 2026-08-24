using ClonEbay_CoreAPI.Common.Models;
using ClonEbay_CoreAPI.DTOs.Notification;
using ClonEbay_CoreAPI.DTOs.Review;
using ClonEbay_CoreAPI.Exceptions;
using ClonEbay_CoreAPI.Models;
using ClonEbay_CoreAPI.Repositories.Interfaces;
using ClonEbay_CoreAPI.Services.Interfaces;

namespace ClonEbay_CoreAPI.Services.Implementations
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepo;
        private readonly IGenericRepository<Product> _productRepo;
        private readonly INotificationService _notificationService;

        public ReviewService(
            IReviewRepository reviewRepo,
            IGenericRepository<Product> productRepo,
            INotificationService notificationService)
        {
            _reviewRepo = reviewRepo;
            _productRepo = productRepo;
            _notificationService = notificationService;
        }

        // ─── GET: Danh sách review của 1 sản phẩm ────────────────────────────────
        public async Task<ApiResponse<ProductReviewSummaryDto>> GetProductReviewsAsync(int productId)
        {
            var product = await _productRepo.GetByIdAsync(productId)
                          ?? throw new NotFoundException("Không tìm thấy sản phẩm.");

            var reviews = await _reviewRepo.GetByProductIdAsync(productId);

            var summary = new ProductReviewSummaryDto
            {
                ProductId = productId,
                TotalReviews = reviews.Count,
                AverageRating = reviews.Any() ? Math.Round(reviews.Average(r => r.Rating ?? 0), 1) : 0,
                Reviews = reviews.Select(ToDto).ToList()
            };

            return ApiResponse<ProductReviewSummaryDto>.Ok(summary);
        }

        // ─── GET: Danh sách review do chính mình đã tạo ────────────────────────
        public async Task<ApiResponse<List<ReviewDto>>> GetMyReviewsAsync(int userId)
        {
            var reviews = await _reviewRepo.GetByReviewerIdAsync(userId);
            return ApiResponse<List<ReviewDto>>.Ok(reviews.Select(ToDto).ToList());
        }

        // ─── POST: Tạo đánh giá mới ─────────────────────────────────────────────
        public async Task<ApiResponse<ReviewDto>> CreateReviewAsync(int userId, CreateReviewDto dto)
        {
            // 1. Kiểm tra sản phẩm có tồn tại hay không
            var product = await _productRepo.GetByIdAsync(dto.ProductId)
                          ?? throw new NotFoundException("Không tìm thấy sản phẩm.");

            // 2. Kiểm tra xem người dùng đã mua sản phẩm và đơn hàng đã giao thành công (Delivered) chưa
            var hasPurchased = await _reviewRepo.HasUserPurchasedProductAsync(userId, dto.ProductId);
            if (!hasPurchased)
            {
                throw new BadRequestException("Bạn chỉ có thể đánh giá sản phẩm sau khi đã mua và nhận hàng thành công (Delivered).");
            }

            // 3. Kiểm tra xem người dùng đã đánh giá sản phẩm này chưa
            var alreadyReviewed = await _reviewRepo.HasUserReviewedProductAsync(userId, dto.ProductId);
            if (alreadyReviewed)
            {
                throw new BadRequestException("Bạn đã gửi đánh giá cho sản phẩm này rồi. Bạn có thể cập nhật đánh giá đã có.");
            }

            // 4. Tạo review mới
            var review = new Review
            {
                ProductId = dto.ProductId,
                ReviewerId = userId,
                Rating = dto.Rating,
                Comment = dto.Comment?.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            await _reviewRepo.AddAsync(review);
            await _reviewRepo.SaveChangesAsync();

            // Refetch để lấy đầy đủ navigation properties (Product, Reviewer) cho DTO
            var createdReview = await _reviewRepo.GetByIdAsync(review.Id);
            var resultDto = ToDto(createdReview ?? review);

            // Gửi thông báo Real-time qua SignalR cho Seller sở hữu sản phẩm
            var sellerId = product.SellerId ?? 0;
            _ = Task.Run(() => _notificationService.SendFeedbackNotificationAsync(new FeedbackNotificationDto
            {
                ReviewId = review.Id,
                ProductId = dto.ProductId,
                ProductTitle = product.Title ?? string.Empty,
                SellerId = sellerId,
                ReviewerName = resultDto.ReviewerName,
                Rating = dto.Rating,
                Comment = dto.Comment ?? string.Empty,
                Message = $"Sản phẩm '{product.Title}' của bạn vừa nhận được đánh giá {dto.Rating} sao từ {resultDto.ReviewerName}."
            }));

            return ApiResponse<ReviewDto>.Ok(resultDto, "Gửi đánh giá sản phẩm thành công.");
        }

        // ─── PUT: Cập nhật đánh giá ──────────────────────────────────────────────
        public async Task<ApiResponse<ReviewDto>> UpdateReviewAsync(int userId, int reviewId, UpdateReviewDto dto)
        {
            var review = await _reviewRepo.GetByIdAsync(reviewId)
                         ?? throw new NotFoundException("Không tìm thấy đánh giá.");

            if (review.ReviewerId != userId)
            {
                throw new ForbiddenException("Bạn không có quyền chỉnh sửa đánh giá này.");
            }

            review.Rating = dto.Rating;
            review.Comment = dto.Comment?.Trim();
            await _reviewRepo.SaveChangesAsync();

            return ApiResponse<ReviewDto>.Ok(ToDto(review), "Cập nhật đánh giá thành công.");
        }

        // ─── DELETE: Xóa đánh giá ───────────────────────────────────────────────
        public async Task<ApiResponse<bool>> DeleteReviewAsync(int userId, string role, int reviewId)
        {
            var review = await _reviewRepo.GetByIdAsync(reviewId)
                         ?? throw new NotFoundException("Không tìm thấy đánh giá.");

            var isAdmin = string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);
            var isOwner = review.ReviewerId == userId;

            if (!isOwner && !isAdmin)
            {
                throw new ForbiddenException("Bạn không có quyền xóa đánh giá này.");
            }

            _reviewRepo.Delete(review);
            await _reviewRepo.SaveChangesAsync();

            return ApiResponse<bool>.Ok(true, "Xóa đánh giá thành công.");
        }

        // ─── Private helpers ────────────────────────────────────────────────────
        private static ReviewDto ToDto(Review r)
        {
            return new ReviewDto
            {
                Id = r.Id,
                ProductId = r.ProductId ?? 0,
                ProductTitle = r.Product?.Title ?? string.Empty,
                ReviewerId = r.ReviewerId ?? 0,
                ReviewerName = r.Reviewer?.Username ?? r.Reviewer?.FullName ?? string.Empty,
                Rating = r.Rating ?? 0,
                Comment = r.Comment ?? string.Empty,
                CreatedAt = r.CreatedAt ?? DateTime.UtcNow
            };
        }
    }
}
