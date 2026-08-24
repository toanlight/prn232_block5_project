using ClonEbay_CoreAPI.Models;

namespace ClonEbay_CoreAPI.Repositories.Interfaces
{
    public interface IReviewRepository
    {
        /// <summary>Lấy danh sách tất cả đánh giá của 1 sản phẩm.</summary>
        Task<List<Review>> GetByProductIdAsync(int productId);

        /// <summary>Lấy danh sách tất cả đánh giá do 1 user viết.</summary>
        Task<List<Review>> GetByReviewerIdAsync(int reviewerId);

        /// <summary>Lấy thông tin chi tiết 1 đánh giá theo Id.</summary>
        Task<Review?> GetByIdAsync(int id);

        /// <summary>Kiểm tra xem user đã mua sản phẩm và đơn hàng có Status = Delivered hay chưa.</summary>
        Task<bool> HasUserPurchasedProductAsync(int userId, int productId);

        /// <summary>Kiểm tra xem user đã từng gửi đánh giá cho sản phẩm này chưa.</summary>
        Task<bool> HasUserReviewedProductAsync(int userId, int productId);

        Task AddAsync(Review review);
        void Delete(Review review);
        Task SaveChangesAsync();
    }
}
