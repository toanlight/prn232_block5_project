using ClonEbay_CoreAPI.Models;
using ClonEbay_CoreAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClonEbay_CoreAPI.Repositories.Implementations
{
    public class CouponRepository : ICouponRepository
    {
        private readonly CloneEbayDbContext _context;
        private readonly DbSet<Coupon> _coupons;

        public CouponRepository(CloneEbayDbContext context)
        {
            _context = context;
            _coupons = context.Coupons;
        }

        public Task<List<Coupon>> GetActiveCouponsAsync()
        {
            var now = DateTime.UtcNow;
            return _coupons
                .AsNoTracking()
                .Include(c => c.Product)
                .Where(c => (c.EndDate == null || c.EndDate >= now)
                         && (c.MaxUsage == null || c.MaxUsage > 0))
                .OrderByDescending(c => c.Id)
                .ToListAsync();
        }

        public Task<Coupon?> GetByCodeAndProductAsync(string code, int productId)
        {
            return _coupons
                .Include(c => c.Product)
                .FirstOrDefaultAsync(c => string.Equals(c.Code, code) && c.ProductId == productId);
        }

        public Task<Coupon?> GetByCodeAsync(string code)
        {
            return _coupons
                .AsNoTracking()
                .FirstOrDefaultAsync(c => string.Equals(c.Code, code));
        }

        public Task<Coupon?> GetByIdAsync(int id)
        {
            return _coupons
                .Include(c => c.Product)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public Task AddAsync(Coupon coupon)
        {
            return _coupons.AddAsync(coupon).AsTask();
        }

        public void Delete(Coupon coupon)
        {
            _coupons.Remove(coupon);
        }

        public Task SaveChangesAsync()
        {
            return _context.SaveChangesAsync();
        }
    }
}
