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
                .Include(r => r.Order)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public Task<List<Models.ReturnRequest>> GetAllAsync()
        {
            return _returnRequests
                .AsNoTracking()
                .Include(r => r.Order)
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public Task<Models.ReturnRequest?> GetByIdAsync(int id)
        {
            return _returnRequests
                .Include(r => r.Order)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public Task<bool> HasPendingRequestAsync(int orderId)
        {
            return _returnRequests
                .AnyAsync(r => r.OrderId == orderId
                               && r.Status == nameof(ReturnRequestStatus.Pending));
        }

        public Task<OrderTable?> GetOrderByIdAsync(int orderId)
        {
            return _context.OrderTables
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
