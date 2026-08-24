using ClonEbay_CoreAPI.Common.Models;
using ClonEbay_CoreAPI.DTOs.ReturnRequest;

namespace ClonEbay_CoreAPI.Services.Interfaces
{
    public interface IReturnRequestService
    {
        /// <summary>Buyer xem danh sách yêu cầu hoàn trả của mình.</summary>
        Task<ApiResponse<List<ReturnRequestDto>>> GetMyRequestsAsync(int userId);

        /// <summary>
        /// Xem chi tiết 1 yêu cầu. Buyer chỉ xem được của mình;
        /// Seller/Admin xem được tất cả.
        /// </summary>
        Task<ApiResponse<ReturnRequestDto>> GetByIdAsync(int requestId, int userId, string role);

        /// <summary>Buyer gửi yêu cầu hoàn trả mới.</summary>
        Task<ApiResponse<ReturnRequestDto>> CreateAsync(int userId, CreateReturnRequestDto dto);

        /// <summary>Buyer huỷ yêu cầu đang Pending của mình.</summary>
        Task<ApiResponse<ReturnRequestDto>> CancelAsync(int requestId, int userId);

        /// <summary>Seller/Admin duyệt hoặc từ chối yêu cầu.</summary>
        Task<ApiResponse<ReturnRequestDto>> UpdateStatusAsync(int requestId, UpdateReturnRequestStatusDto dto, string role);
    }
}
