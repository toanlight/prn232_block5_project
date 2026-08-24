using ClonEbay_CoreAPI.Models;
using ClonEbay_CoreAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClonEbay_CoreAPI.Repositories.Implementations
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly CloneEbayDbContext _context;
        private readonly DbSet<Review> _reviews;

        public ReviewRepository(CloneEbayDbContext context)
        {
            _context = context;
            _reviews = context.Reviews;
        }

        public Task<List<Review>> GetByProductIdAsync(int productId)
        {
            return _reviews
                .AsNoTracking()
                .Include(r => r.Product)
                .Include(r => r.Reviewer)
                .Where(r => r.ProductId == productId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public Task<List<Review>> GetByReviewerIdAsync(int reviewerId)
        {
            return _reviews
                .AsNoTracking()
                .Include(r => r.Product)
                .Include(r => r.Reviewer)
                .Where(r => r.ReviewerId == reviewerId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public Task<Review?> GetByIdAsync(int id)
        {
            return _reviews
                .Include(r => r.Product)
                .Include(r => r.Reviewer)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public Task<bool> HasUserPurchasedProductAsync(int userId, int productId)
        {
            return _context.OrderTables
                .AsNoTracking()
                .Where(o => o.BuyerId == userId && string.Equals(o.Status, "Delivered"))
                .SelectMany(o => o.OrderItems)
                .AnyAsync(item => item.ProductId == productId);
        }

        public Task<bool> HasUserReviewedProductAsync(int userId, int productId)
        {
            return _reviews
                .AnyAsync(r => r.ReviewerId == userId && r.ProductId == productId);
        }

        public Task AddAsync(Review review)
        {
            return _reviews.AddAsync(review).AsTask();
        }

        public void Delete(Review review)
        {
            _reviews.Remove(review);
        }

        public Task SaveChangesAsync()
        {
            return _context.SaveChangesAsync();
        }
    }
}
