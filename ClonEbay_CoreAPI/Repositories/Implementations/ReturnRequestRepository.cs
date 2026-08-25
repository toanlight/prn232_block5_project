using ClonEbay_CoreAPI.Models;
using ClonEbay_CoreAPI.Models.Enums;
using ClonEbay_CoreAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClonEbay_CoreAPI.Repositories.Implementations
{
    public class ReturnRequestRepository : IReturnRequestRepository
    {
        private readonly CloneEbayDbContext _context;
        private readonly DbSet<Models.ReturnRequest> _returnRequests;

        public ReturnRequestRepository(CloneEbayDbContext context)
        {
            _context = context;
            _returnRequests = context.ReturnRequests;
        }

        public Task<List<Models.ReturnRequest>> GetByUserIdAsync(int userId)
        {
            return _returnRequests
                .AsNoTracking()
                .AsSplitQuery()
                .Include(r => r.Order)
                .Include(r => r.OrderItem)
                .Include(r => r.Product)
                    .ThenInclude(p => p!.Seller)
                .Include(r => r.Evidences)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public Task<List<Models.ReturnRequest>> GetBySellerIdAsync(int sellerId)
        {
            return _returnRequests
                .AsNoTracking()
                .AsSplitQuery()
                .Include(r => r.Order)
                .Include(r => r.OrderItem)
                .Include(r => r.Product)
                    .ThenInclude(p => p!.Seller)
                .Include(r => r.User)
                .Include(r => r.Evidences)
                .Where(r => r.Product != null && r.Product.SellerId == sellerId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public Task<List<Models.ReturnRequest>> GetAllAsync()
        {
            return _returnRequests
                .AsNoTracking()
                .AsSplitQuery()
                .Include(r => r.Order)
                .Include(r => r.OrderItem)
                .Include(r => r.Product)
                    .ThenInclude(p => p!.Seller)
                .Include(r => r.User)
                .Include(r => r.Evidences)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public Task<Models.ReturnRequest?> GetByIdAsync(int id)
        {
            return _returnRequests
                .AsSplitQuery()
                .Include(r => r.Order)
                .Include(r => r.OrderItem)
                .Include(r => r.Product)
                    .ThenInclude(p => p!.Seller)
                .Include(r => r.User)
                .Include(r => r.Evidences)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public Task<bool> HasPendingRequestAsync(int orderId, int? orderItemId = null)
        {
            var query = _returnRequests.Where(r => r.OrderId == orderId &&
                (r.Status == nameof(ReturnRequestStatus.Requested) ||
                 r.Status == nameof(ReturnRequestStatus.Pending) ||
                 r.Status == nameof(ReturnRequestStatus.Approved) ||
                 r.Status == nameof(ReturnRequestStatus.Returning) ||
                 r.Status == nameof(ReturnRequestStatus.Returned) ||
                 r.Status == nameof(ReturnRequestStatus.Escalated)));

            if (orderItemId.HasValue && orderItemId.Value > 0)
            {
                query = query.Where(r => r.OrderItemId == orderItemId.Value);
            }

            return query.AnyAsync();
        }

        public Task<OrderTable?> GetOrderByIdAsync(int orderId)
        {
            return _context.OrderTables
                .AsSplitQuery()
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public Task AddAsync(Models.ReturnRequest entity)
        {
            return _returnRequests.AddAsync(entity).AsTask();
        }

        public Task SaveChangesAsync()
        {
            return _context.SaveChangesAsync();
        }
    }
}
