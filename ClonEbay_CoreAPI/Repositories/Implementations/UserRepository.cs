using ClonEbay_CoreAPI.Models;
using ClonEbay_CoreAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClonEbay_CoreAPI.Repositories.Implementations
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(CloneEbayDbContext context) : base(context)
        {
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            var normalizedEmail = email.Trim().ToLower();
            return await _dbSet.FirstOrDefaultAsync(u => u.Email != null && u.Email.ToLower() == normalizedEmail);
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            var normalizedUsername = username.Trim().ToLower();
            return await _dbSet.FirstOrDefaultAsync(u => u.Username != null && u.Username.ToLower() == normalizedUsername);
        }

        public async Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail)
        {
            var normalized = usernameOrEmail.Trim().ToLower();
            return await _dbSet.FirstOrDefaultAsync(u => 
                (u.Email != null && u.Email.ToLower() == normalized) || 
                (u.Username != null && u.Username.ToLower() == normalized));
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            var normalizedEmail = email.Trim().ToLower();
            return await _dbSet.AnyAsync(u => u.Email != null && u.Email.ToLower() == normalizedEmail);
        }

        public async Task<bool> ExistsByUsernameAsync(string username)
        {
            var normalizedUsername = username.Trim().ToLower();
            return await _dbSet.AnyAsync(u => u.Username != null && u.Username.ToLower() == normalizedUsername);
        }

        public async Task<User?> GetUserWithAddressesAsync(int userId)
        {
            return await _dbSet
                .Include(u => u.Addresses)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<List<OrderTable>> GetUserOrdersAsync(int userId)
        {
            return await _context.OrderTables
                .AsNoTracking()
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.ReturnRequests)
                .Where(o => o.BuyerId == userId)
                .OrderByDescending(o => o.OrderDate ?? DateTime.MinValue)
                .ToListAsync();
        }

        public async Task<List<int>> GetUserReviewedProductIdsAsync(int userId)
        {
            return await _context.Reviews
                .AsNoTracking()
                .Where(r => r.ReviewerId == userId && r.ProductId.HasValue)
                .Select(r => r.ProductId!.Value)
                .ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
