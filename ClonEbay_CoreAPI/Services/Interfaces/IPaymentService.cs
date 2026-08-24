using ClonEbay_CoreAPI.Common.Models;
using ClonEbay_CoreAPI.DTOs.Payment;

namespace ClonEbay_CoreAPI.Services.Interfaces;

public interface IPaymentService
{
    Task<ApiResponse<PayPalPaymentDto>> GetPayPalPaymentAsync(int userId, int orderId);
    Task<ApiResponse<PayPalPaymentDto>> SimulatePayPalAsync(
        int userId,
        int orderId,
        SimulatePayPalRequestDto request);
}
