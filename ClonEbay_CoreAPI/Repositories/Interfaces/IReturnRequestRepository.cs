using ClonEbay_CoreAPI.Models;

namespace ClonEbay_CoreAPI.Repositories.Interfaces
{
    public interface IReturnRequestRepository
    {
        Task<List<Models.ReturnRequest>> GetByUserIdAsync(int userId);
        Task<List<Models.ReturnRequest>> GetBySellerIdAsync(int sellerId);
        Task<List<Models.ReturnRequest>> GetAllAsync();
        Task<Models.ReturnRequest?> GetByIdAsync(int id);
        Task<bool> HasPendingRequestAsync(int orderId, int? orderItemId = null);
        Task<OrderTable?> GetOrderByIdAsync(int orderId);
        Task AddAsync(Models.ReturnRequest entity);
        Task SaveChangesAsync();
    }
}
