using ClonEbay_CoreAPI.Common.Models;
using ClonEbay_CoreAPI.DTOs.ReturnRequest;
using ClonEbay_CoreAPI.Exceptions;
using ClonEbay_CoreAPI.Models;
using ClonEbay_CoreAPI.Models.Enums;
using ClonEbay_CoreAPI.Repositories.Interfaces;
using ClonEbay_CoreAPI.Services.Interfaces;

namespace ClonEbay_CoreAPI.Services.Implementations
{
    public class ReturnRequestService : IReturnRequestService
    {
        private readonly IReturnRequestRepository _returnRequestRepo;

        public ReturnRequestService(IReturnRequestRepository returnRequestRepo)
        {
            _returnRequestRepo = returnRequestRepo;
        }

        // ─── GET: Danh sách yêu cầu của Buyer ────────────────────────────────
        public async Task<ApiResponse<List<ReturnRequestDto>>> GetMyRequestsAsync(int userId)
        {
            var requests = await _returnRequestRepo.GetByUserIdAsync(userId);
            return ApiResponse<List<ReturnRequestDto>>.Ok(requests.Select(ToDto).ToList());
        }

        // ─── GET: Chi tiết 1 yêu cầu ─────────────────────────────────────────
        public async Task<ApiResponse<ReturnRequestDto>> GetByIdAsync(int requestId, int userId, string role)
        {
            var request = await GetRequestOrThrowAsync(requestId);

            // Buyer chỉ xem được yêu cầu của chính mình
            var isBuyer = role.Equals("User", StringComparison.OrdinalIgnoreCase)
                          || role.Equals("Buyer", StringComparison.OrdinalIgnoreCase);
            if (isBuyer && request.UserId != userId)
            {
                throw new ForbiddenException("Bạn không có quyền xem yêu cầu hoàn trả này.");
            }

            return ApiResponse<ReturnRequestDto>.Ok(ToDto(request));
        }

        // ─── POST: Tạo yêu cầu hoàn trả mới ─────────────────────────────────
        public async Task<ApiResponse<ReturnRequestDto>> CreateAsync(int userId, CreateReturnRequestDto dto)
        {
            // 1. Kiểm tra đơn hàng tồn tại
            var order = await _returnRequestRepo.GetOrderByIdAsync(dto.OrderId)
                        ?? throw new NotFoundException("Không tìm thấy đơn hàng.");

            // 2. Đơn hàng phải thuộc về Buyer đang đăng nhập
            if (order.BuyerId != userId)
            {
                throw new ForbiddenException("Bạn không có quyền gửi yêu cầu hoàn trả cho đơn hàng này.");
            }

            // 3. Chỉ hoàn trả khi đơn hàng đã giao (Delivered)
            if (!string.Equals(order.Status, "Delivered", StringComparison.OrdinalIgnoreCase))
            {
                throw new BadRequestException("Chỉ có thể gửi yêu cầu hoàn trả khi đơn hàng đã được giao thành công.");
            }

            // 4. Kiểm tra đã có yêu cầu Pending chưa
            var hasPending = await _returnRequestRepo.HasPendingRequestAsync(dto.OrderId);
            if (hasPending)
            {
                throw new BadRequestException("Đơn hàng này đã có yêu cầu hoàn trả đang chờ xử lý.");
            }

            // 5. Tạo yêu cầu mới
            var newRequest = new ReturnRequest
            {
                OrderId = dto.OrderId,
                UserId = userId,
                Reason = dto.Reason.Trim(),
                Status = nameof(ReturnRequestStatus.Pending),
                CreatedAt = DateTime.UtcNow
            };

            await _returnRequestRepo.AddAsync(newRequest);
            await _returnRequestRepo.SaveChangesAsync();

            return ApiResponse<ReturnRequestDto>.Ok(ToDto(newRequest), "Gửi yêu cầu hoàn trả thành công. Vui lòng chờ xử lý.");
        }

        // ─── PUT: Buyer huỷ yêu cầu ──────────────────────────────────────────
        public async Task<ApiResponse<ReturnRequestDto>> CancelAsync(int requestId, int userId)
        {
            var request = await GetRequestOrThrowAsync(requestId);

            // Chỉ huỷ yêu cầu của chính mình
            if (request.UserId != userId)
            {
                throw new ForbiddenException("Bạn không có quyền huỷ yêu cầu hoàn trả này.");
            }

            // Chỉ huỷ được khi đang Pending
            if (!string.Equals(request.Status, nameof(ReturnRequestStatus.Pending), StringComparison.OrdinalIgnoreCase))
            {
                throw new BadRequestException("Chỉ có thể huỷ yêu cầu hoàn trả đang chờ xử lý (Pending).");
            }

            request.Status = nameof(ReturnRequestStatus.Cancelled);
            await _returnRequestRepo.SaveChangesAsync();

            return ApiResponse<ReturnRequestDto>.Ok(ToDto(request), "Đã huỷ yêu cầu hoàn trả.");
        }

        // ─── PUT: Seller/Admin duyệt hoặc từ chối ────────────────────────────
        public async Task<ApiResponse<ReturnRequestDto>> UpdateStatusAsync(int requestId, UpdateReturnRequestStatusDto dto, string role)
        {
            // Chỉ Seller và Admin mới được duyệt/từ chối
            var isAllowed = role.Equals("Seller", StringComparison.OrdinalIgnoreCase)
                            || role.Equals("Admin", StringComparison.OrdinalIgnoreCase);
            if (!isAllowed)
            {
                throw new ForbiddenException("Chỉ Seller hoặc Admin mới có thể duyệt/từ chối yêu cầu hoàn trả.");
            }

            var request = await GetRequestOrThrowAsync(requestId);

            // Chỉ xử lý khi đang Pending
            if (!string.Equals(request.Status, nameof(ReturnRequestStatus.Pending), StringComparison.OrdinalIgnoreCase))
            {
                throw new BadRequestException("Chỉ có thể duyệt/từ chối yêu cầu đang chờ xử lý (Pending).");
            }

            request.Status = dto.Status; // "Approved" hoặc "Rejected"
            await _returnRequestRepo.SaveChangesAsync();

            var message = dto.Status == nameof(ReturnRequestStatus.Approved)
                ? "Đã chấp nhận yêu cầu hoàn trả."
                : "Đã từ chối yêu cầu hoàn trả.";

            return ApiResponse<ReturnRequestDto>.Ok(ToDto(request), message);
        }

        // ─── Private helpers ──────────────────────────────────────────────────

        private async Task<ReturnRequest> GetRequestOrThrowAsync(int requestId)
        {
            return await _returnRequestRepo.GetByIdAsync(requestId)
                   ?? throw new NotFoundException("Không tìm thấy yêu cầu hoàn trả.");
        }

        private static ReturnRequestDto ToDto(ReturnRequest r)
        {
            return new ReturnRequestDto
            {
                Id = r.Id,
                OrderId = r.OrderId ?? 0,
                UserId = r.UserId ?? 0,
                Reason = r.Reason ?? string.Empty,
                Status = r.Status ?? string.Empty,
                CreatedAt = r.CreatedAt ?? DateTime.UtcNow
            };
        }
    }
}
