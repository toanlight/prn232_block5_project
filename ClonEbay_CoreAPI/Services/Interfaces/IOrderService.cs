using ClonEbay_CoreAPI.Common.Models;
using ClonEbay_CoreAPI.DTOs.Order;

namespace ClonEbay_CoreAPI.Services.Interfaces;

public interface IOrderService
{
    Task<ApiResponse<CheckoutDto>> GetCheckoutAsync(int userId, int? addressId = null, Dictionary<int, string>? appliedCoupons = null);
    Task<ApiResponse<OrderCreatedDto>> PlaceOrderAsync(int userId, PlaceOrderRequestDto request);
    Task<ApiResponse<List<OrderDto>>> GetUserOrdersAsync(int userId);
    Task<ApiResponse<OrderDto>> ConfirmReceiptAsync(int userId, int orderId);
}
