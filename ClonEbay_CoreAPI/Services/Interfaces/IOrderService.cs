using ClonEbay_CoreAPI.Common.Models;
using ClonEbay_CoreAPI.DTOs.Order;

namespace ClonEbay_CoreAPI.Services.Interfaces;

public interface IOrderService
{
    Task<ApiResponse<CheckoutDto>> GetCheckoutAsync(int userId, int? addressId = null);
    Task<ApiResponse<OrderCreatedDto>> PlaceOrderAsync(int userId, PlaceOrderRequestDto request);
    Task<ApiResponse<List<OrderHistoryItemDto>>> GetMyOrdersAsync(int userId);
    Task<ApiResponse<OrderHistoryItemDto>> GetOrderDetailAsync(int userId, int orderId);
}
