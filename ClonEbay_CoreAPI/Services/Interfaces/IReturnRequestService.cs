using ClonEbay_CoreAPI.Common.Models;
using ClonEbay_CoreAPI.DTOs.ReturnRequest;

namespace ClonEbay_CoreAPI.Services.Interfaces
{
    public interface IReturnRequestService
    {
        Task<ApiResponse<List<ReturnRequestDto>>> GetMyRequestsAsync(int userId);
        Task<ApiResponse<List<ReturnRequestDto>>> GetSellerRequestsAsync(int sellerId);
        Task<ApiResponse<ReturnRequestDto>> GetByIdAsync(int requestId, int userId, string role);
        Task<ApiResponse<List<ReturnRequestDto>>> CreateAsync(int userId, CreateReturnRequestDto dto);
        Task<ApiResponse<ReturnRequestDto>> CancelAsync(int requestId, int userId);
        Task<ApiResponse<ReturnRequestDto>> UpdateStatusAsync(int requestId, UpdateReturnRequestStatusDto dto, int currentUserId, string role);
        Task<ApiResponse<ReturnRequestDto>> UpdateTrackingAsync(int requestId, int userId, UpdateReturnTrackingDto dto);
        Task<ApiResponse<ReturnRequestDto>> ConfirmItemReturnedAsync(int requestId, int userId, string role);
        Task<ApiResponse<ReturnRequestDto>> EscalateToAdminAsync(int requestId, int userId, EscalateReturnDto dto);
    }
}
