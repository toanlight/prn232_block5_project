using ClonEbay_CoreAPI.Common.Models;
using ClonEbay_CoreAPI.DTOs.Order;

namespace ClonEbay_CoreAPI.Services.Interfaces;

public interface IOrderService
{
    Task<ApiResponse<OrderHistoryPageDto>> GetOrderHistoryAsync(
        int userId,
        int page = 1,
        int pageSize = 10,
        string? status = null);
    Task<ApiResponse<OrderDetailDto>> GetOrderDetailAsync(int userId, int orderId);
    Task<ApiResponse<CheckoutDto>> GetCheckoutAsync(int userId, int? addressId = null);
    Task<ApiResponse<OrderCreatedDto>> PlaceOrderAsync(int userId, PlaceOrderRequestDto request);
}
