using ClonEbay_CoreAPI.Models;

namespace ClonEbay_CoreAPI.Repositories.Interfaces
{
    public interface ICouponRepository
    {
        /// <summary>Lấy danh sách tất cả coupon còn hiệu lực (EndDate >= Now và MaxUsage > 0).</summary>
        Task<List<Coupon>> GetActiveCouponsAsync();

        /// <summary>Tìm coupon theo code và productId.</summary>
        Task<Coupon?> GetByCodeAndProductAsync(string code, int productId);

        /// <summary>Tìm coupon theo code (để check trùng mã khi tạo).</summary>
        Task<Coupon?> GetByCodeAsync(string code);

        /// <summary>Lấy thông tin coupon theo Id.</summary>
        Task<Coupon?> GetByIdAsync(int id);

        Task AddAsync(Coupon coupon);
        void Delete(Coupon coupon);
        Task SaveChangesAsync();
    }
}
