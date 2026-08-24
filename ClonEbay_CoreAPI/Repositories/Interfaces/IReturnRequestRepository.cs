using ClonEbay_CoreAPI.Models;

namespace ClonEbay_CoreAPI.Repositories.Interfaces
{
    public interface IReturnRequestRepository
    {
        /// <summary>Lấy tất cả yêu cầu hoàn trả của một Buyer.</summary>
        Task<List<Models.ReturnRequest>> GetByUserIdAsync(int userId);

        /// <summary>Lấy tất cả yêu cầu hoàn trả (dành cho Seller/Admin).</summary>
        Task<List<Models.ReturnRequest>> GetAllAsync();

        /// <summary>Lấy yêu cầu hoàn trả theo Id (kèm navigation properties).</summary>
        Task<Models.ReturnRequest?> GetByIdAsync(int id);

        /// <summary>Kiểm tra đơn hàng đã có yêu cầu hoàn trả đang Pending chưa.</summary>
        Task<bool> HasPendingRequestAsync(int orderId);

        /// <summary>Lấy đơn hàng theo Id để validate điều kiện hoàn trả.</summary>
        Task<OrderTable?> GetOrderByIdAsync(int orderId);

        Task AddAsync(Models.ReturnRequest entity);
        Task SaveChangesAsync();
    }
}
