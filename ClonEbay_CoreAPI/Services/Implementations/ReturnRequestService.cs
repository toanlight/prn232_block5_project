using ClonEbay_CoreAPI.Common.Models;
using ClonEbay_CoreAPI.DTOs.Notification;
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
        private readonly INotificationService _notificationService;

        public ReturnRequestService(
            IReturnRequestRepository returnRequestRepo,
            INotificationService notificationService)
        {
            _returnRequestRepo = returnRequestRepo;
            _notificationService = notificationService;
        }

        public async Task<ApiResponse<List<ReturnRequestDto>>> GetMyRequestsAsync(int userId)
        {
            var requests = await _returnRequestRepo.GetByUserIdAsync(userId);
            return ApiResponse<List<ReturnRequestDto>>.Ok(requests.Select(ToDto).ToList());
        }

        public async Task<ApiResponse<List<ReturnRequestDto>>> GetSellerRequestsAsync(int sellerId)
        {
            var requests = await _returnRequestRepo.GetBySellerIdAsync(sellerId);
            return ApiResponse<List<ReturnRequestDto>>.Ok(requests.Select(ToDto).ToList());
        }

        public async Task<ApiResponse<ReturnRequestDto>> GetByIdAsync(int requestId, int userId, string role)
        {
            var request = await GetRequestOrThrowAsync(requestId);

            var isBuyer = role.Equals("User", StringComparison.OrdinalIgnoreCase) || role.Equals("Buyer", StringComparison.OrdinalIgnoreCase);
            var isSeller = role.Equals("Seller", StringComparison.OrdinalIgnoreCase);

            if (isBuyer && request.UserId != userId)
            {
                throw new ForbiddenException("Bạn không có quyền xem yêu cầu hoàn trả này.");
            }

            if (isSeller && request.Product?.SellerId != userId)
            {
                throw new ForbiddenException("Bạn không có quyền xem yêu cầu hoàn trả của shop khác.");
            }

            return ApiResponse<ReturnRequestDto>.Ok(ToDto(request));
        }

        public async Task<ApiResponse<List<ReturnRequestDto>>> CreateAsync(int userId, CreateReturnRequestDto dto)
        {
            // 1. Check Order tồn tại
            var order = await _returnRequestRepo.GetOrderByIdAsync(dto.OrderId)
                        ?? throw new NotFoundException("Không tìm thấy đơn hàng.");

            // 2. Check Buyer có phải owner Order
            if (order.BuyerId != userId)
            {
                throw new ForbiddenException("Bạn không có quyền gửi yêu cầu hoàn trả cho đơn hàng này.");
            }

            // 3. Check Order Status có được return?
            if (!string.Equals(order.Status, "Delivered", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(order.Status, "Return Requested", StringComparison.OrdinalIgnoreCase))
            {
                throw new BadRequestException("Chỉ có thể gửi yêu cầu hoàn trả khi đơn hàng đã được giao thành công (Delivered).");
            }

            // 4. ReturnEntireOrder? (Rẽ nhánh YES / NO)
            List<OrderItem> targetItems;
            if (dto.ReturnEntireOrder)
            {
                targetItems = order.OrderItems.ToList();
            }
            else if (dto.SelectedOrderItemIds != null && dto.SelectedOrderItemIds.Any())
            {
                targetItems = order.OrderItems.Where(oi => dto.SelectedOrderItemIds.Contains(oi.Id)).ToList();
            }
            else if (dto.OrderItemId.HasValue && dto.OrderItemId.Value > 0)
            {
                targetItems = order.OrderItems.Where(oi => oi.Id == dto.OrderItemId.Value).ToList();
            }
            else
            {
                targetItems = order.OrderItems.Take(1).ToList();
            }

            if (!targetItems.Any())
            {
                throw new BadRequestException("Vui lòng chọn ít nhất một sản phẩm để gửi yêu cầu hoàn trả.");
            }

            // 5. Check Item đã có active return & Create ReturnRequest for EACH OrderItem
            var createdRequests = new List<ReturnRequest>();
            var skippedItemTitles = new List<string>();

            foreach (var item in targetItems)
            {
                var hasPending = await _returnRequestRepo.HasPendingRequestAsync(dto.OrderId, item.Id);
                if (hasPending)
                {
                    skippedItemTitles.Add(item.Product?.Title ?? $"Mặt hàng #{item.Id}");
                    continue;
                }

                var newRequest = new ReturnRequest
                {
                    OrderId = dto.OrderId,
                    OrderItemId = item.Id,
                    ProductId = item.ProductId,
                    UserId = userId,
                    Reason = dto.Reason.Trim(),
                    Status = nameof(ReturnRequestStatus.Requested),
                    CreatedAt = DateTime.UtcNow
                };

                if (dto.Evidences != null && dto.Evidences.Any())
                {
                    foreach (var img in dto.Evidences)
                    {
                        if (!string.IsNullOrWhiteSpace(img))
                        {
                            newRequest.Evidences.Add(new ReturnEvidence
                            {
                                FileUrl = img.Trim(),
                                UploadedAt = DateTime.UtcNow
                            });
                        }
                    }
                }

                await _returnRequestRepo.AddAsync(newRequest);
                createdRequests.Add(newRequest);
            }

            if (!createdRequests.Any())
            {
                throw new BadRequestException($"Các sản phẩm được chọn ({string.Join(", ", skippedItemTitles)}) đều đã có yêu cầu hoàn trả đang được xử lý.");
            }

            // 6. Save Database
            order.Status = "Return Requested";
            await _returnRequestRepo.SaveChangesAsync();

            // 7. Create Notification & SignalR -> Seller
            foreach (var req in createdRequests)
            {
                var sellerId = req.Product?.SellerId ?? 0;
                if (sellerId > 0)
                {
                    await _notificationService.SendOrderNotificationAsync(new OrderNotificationDto
                    {
                        OrderId = order.Id,
                        BuyerId = userId,
                        SellerId = sellerId,
                        TotalPrice = order.TotalPrice ?? 0,
                        Status = "Return Requested",
                        Message = $"[Yêu cầu hoàn trả #{req.Id}] Người mua đã gửi yêu cầu trả sản phẩm: '{req.Product?.Title}'. Lý do: {dto.Reason}"
                    });
                }
            }

            var message = createdRequests.Count > 1
                ? $"Đã tạo thành công {createdRequests.Count} yêu cầu hoàn trả cho các sản phẩm đã chọn."
                : "Gửi yêu cầu hoàn trả sản phẩm thành công. Vui lòng chờ phản hồi từ Người bán.";

            return ApiResponse<List<ReturnRequestDto>>.Ok(createdRequests.Select(ToDto).ToList(), message);
        }

        public async Task<ApiResponse<ReturnRequestDto>> CancelAsync(int requestId, int userId)
        {
            var request = await GetRequestOrThrowAsync(requestId);

            if (request.UserId != userId)
            {
                throw new ForbiddenException("Bạn không có quyền huỷ yêu cầu hoàn trả này.");
            }

            if (string.Equals(request.Status, nameof(ReturnRequestStatus.Returned), StringComparison.OrdinalIgnoreCase) ||
                string.Equals(request.Status, nameof(ReturnRequestStatus.Refunded), StringComparison.OrdinalIgnoreCase))
            {
                throw new BadRequestException("Không thể huỷ yêu cầu hoàn trả đã hoàn tất.");
            }

            request.Status = nameof(ReturnRequestStatus.Cancelled);

            if (request.Order != null)
            {
                request.Order.Status = "Delivered";
            }

            await _returnRequestRepo.SaveChangesAsync();

            var sellerId = request.Product?.SellerId ?? 0;
            if (sellerId > 0)
            {
                await _notificationService.SendOrderNotificationAsync(new OrderNotificationDto
                {
                    OrderId = request.OrderId ?? 0,
                    BuyerId = userId,
                    SellerId = sellerId,
                    Status = "Return Cancelled",
                    Message = $"Người mua đã huỷ yêu cầu hoàn trả sản phẩm #{request.Id}."
                });
            }

            return ApiResponse<ReturnRequestDto>.Ok(ToDto(request), "Đã huỷ yêu cầu hoàn trả.");
        }

        public async Task<ApiResponse<ReturnRequestDto>> UpdateStatusAsync(int requestId, UpdateReturnRequestStatusDto dto, int currentUserId, string role)
        {
            var request = await GetRequestOrThrowAsync(requestId);

            var isSeller = role.Equals("Seller", StringComparison.OrdinalIgnoreCase);
            var isAdmin = role.Equals("Admin", StringComparison.OrdinalIgnoreCase);

            if (!isSeller && !isAdmin)
            {
                throw new ForbiddenException("Chỉ Seller hoặc Admin mới có thể xử lý yêu cầu hoàn trả.");
            }

            if (isSeller && request.Product?.SellerId != currentUserId)
            {
                throw new ForbiddenException("Bạn không có quyền xử lý yêu cầu hoàn trả của shop khác.");
            }

            request.Status = dto.Status;
            request.AdminNotes = dto.AdminNotes;

            if (dto.Status == nameof(ReturnRequestStatus.Approved))
            {
                if (request.Order != null) request.Order.Status = "Return Approved";
            }
            else if (dto.Status == nameof(ReturnRequestStatus.Rejected))
            {
                if (request.Order != null) request.Order.Status = "Delivered";
            }
            else if (dto.Status == nameof(ReturnRequestStatus.Refunded))
            {
                request.RefundType = dto.RefundType ?? "Full";
                var unitPrice = request.OrderItem?.UnitPrice ?? request.Order?.TotalPrice ?? 0;
                request.RefundAmount = dto.RefundAmount ?? unitPrice;
                if (request.Order != null) request.Order.Status = "Returned";
            }

            await _returnRequestRepo.SaveChangesAsync();

            await _notificationService.SendOrderNotificationAsync(new OrderNotificationDto
            {
                OrderId = request.OrderId ?? 0,
                BuyerId = request.UserId ?? 0,
                SellerId = request.Product?.SellerId ?? 0,
                Status = request.Status,
                Message = $"Yêu cầu hoàn trả #{request.Id} đã được cập nhật trạng thái: {request.Status}."
            });

            return ApiResponse<ReturnRequestDto>.Ok(ToDto(request), $"Đã cập nhật trạng thái yêu cầu thành: {request.Status}");
        }

        public async Task<ApiResponse<ReturnRequestDto>> UpdateTrackingAsync(int requestId, int userId, UpdateReturnTrackingDto dto)
        {
            var request = await GetRequestOrThrowAsync(requestId);

            if (request.UserId != userId)
            {
                throw new ForbiddenException("Bạn không có quyền cập nhật mã vận chuyển cho yêu cầu này.");
            }

            request.TrackingNumber = dto.TrackingNumber.Trim();
            request.Status = nameof(ReturnRequestStatus.Returning);

            if (request.Order != null)
            {
                request.Order.Status = "Item Returning";
            }

            await _returnRequestRepo.SaveChangesAsync();

            var sellerId = request.Product?.SellerId ?? 0;
            if (sellerId > 0)
            {
                await _notificationService.SendOrderNotificationAsync(new OrderNotificationDto
                {
                    OrderId = request.OrderId ?? 0,
                    BuyerId = userId,
                    SellerId = sellerId,
                    Status = "Item Returning",
                    Message = $"Người mua đã gửi trả lại hàng với Mã vận chuyển: {dto.TrackingNumber}"
                });
            }

            return ApiResponse<ReturnRequestDto>.Ok(ToDto(request), "Cập nhật mã vận chuyển trả hàng thành công.");
        }

        public async Task<ApiResponse<ReturnRequestDto>> ConfirmItemReturnedAsync(int requestId, int userId, string role)
        {
            var request = await GetRequestOrThrowAsync(requestId);

            var isSeller = role.Equals("Seller", StringComparison.OrdinalIgnoreCase);
            var isAdmin = role.Equals("Admin", StringComparison.OrdinalIgnoreCase);

            if (!isSeller && !isAdmin)
            {
                throw new ForbiddenException("Chỉ Seller hoặc Admin mới có thể xác nhận đã nhận hàng trả về.");
            }

            request.Status = nameof(ReturnRequestStatus.Refunded);
            request.RefundType = "Full";
            request.RefundAmount = request.OrderItem?.UnitPrice ?? request.Order?.TotalPrice ?? 0;

            if (request.Order != null)
            {
                request.Order.Status = "Returned";
            }

            await _returnRequestRepo.SaveChangesAsync();

            await _notificationService.SendOrderNotificationAsync(new OrderNotificationDto
            {
                OrderId = request.OrderId ?? 0,
                BuyerId = request.UserId ?? 0,
                SellerId = request.Product?.SellerId ?? 0,
                Status = "Refunded",
                Message = $"Seller đã nhận lại hàng và hoàn tiền thành công cho yêu cầu #{request.Id}."
            });

            return ApiResponse<ReturnRequestDto>.Ok(ToDto(request), "Xác nhận đã nhận lại sản phẩm và hoàn tiền thành công!");
        }

        public async Task<ApiResponse<ReturnRequestDto>> EscalateToAdminAsync(int requestId, int userId, EscalateReturnDto dto)
        {
            var request = await GetRequestOrThrowAsync(requestId);

            if (request.UserId != userId)
            {
                throw new ForbiddenException("Bạn không có quyền yêu cầu Admin can thiệp cho đơn hàng này.");
            }

            request.IsEscalated = true;
            request.EscalationReason = dto.Reason.Trim();
            request.Status = nameof(ReturnRequestStatus.Escalated);

            await _returnRequestRepo.SaveChangesAsync();

            await _notificationService.SendOrderNotificationAsync(new OrderNotificationDto
            {
                OrderId = request.OrderId ?? 0,
                BuyerId = userId,
                SellerId = request.Product?.SellerId ?? 0,
                Status = "Escalated",
                Message = $"[Ask eBay to step in] Người mua đã yêu cầu Admin can thiệp xử lý tranh chấp hoàn trả #{request.Id}."
            });

            return ApiResponse<ReturnRequestDto>.Ok(ToDto(request), "Đã gửi yêu cầu hỗ trợ tới Ban Quản Trị Admin. eBay sẽ sớm xem xét và can thiệp.");
        }

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
                OrderItemId = r.OrderItemId,
                ProductId = r.ProductId,
                ProductTitle = r.Product?.Title ?? r.OrderItem?.Product?.Title ?? "Sản phẩm",
                ProductImageUrl = ProductService.FirstImage(r.Product?.Images ?? r.OrderItem?.Product?.Images),
                UnitPrice = r.OrderItem?.UnitPrice ?? r.Order?.TotalPrice ?? 0,
                Quantity = r.OrderItem?.Quantity ?? 1,
                UserId = r.UserId ?? 0,
                BuyerName = r.User?.FullName ?? r.User?.Username ?? "Người mua",
                SellerId = r.Product?.SellerId ?? r.OrderItem?.Product?.SellerId,
                SellerName = r.Product?.Seller?.FullName ?? r.Product?.Seller?.Username ?? "Người bán",
                Reason = r.Reason ?? string.Empty,
                Status = r.Status ?? string.Empty,
                RefundAmount = r.RefundAmount,
                RefundType = r.RefundType,
                TrackingNumber = r.TrackingNumber,
                IsEscalated = r.IsEscalated,
                EscalationReason = r.EscalationReason,
                AdminNotes = r.AdminNotes,
                CreatedAt = r.CreatedAt ?? DateTime.UtcNow,
                Evidences = r.Evidences.Select(e => new ReturnEvidenceDto
                {
                    Id = e.Id,
                    FileUrl = e.FileUrl,
                    UploadedAt = e.UploadedAt
                }).ToList()
            };
        }
    }
}
