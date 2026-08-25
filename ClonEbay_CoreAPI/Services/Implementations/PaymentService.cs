using System.Data;
using ClonEbay_CoreAPI.Common.Models;
using ClonEbay_CoreAPI.DTOs.Payment;
using ClonEbay_CoreAPI.Exceptions;
using ClonEbay_CoreAPI.Models;
using ClonEbay_CoreAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClonEbay_CoreAPI.Services.Implementations;

public sealed class PaymentService(CloneEbayDbContext context) : IPaymentService
{
    public async Task<ApiResponse<PayPalPaymentDto>> GetPayPalPaymentAsync(int userId, int orderId)
    {
        var payment = await PaymentQuery(asTracking: false)
            .SingleOrDefaultAsync(item =>
                item.OrderId == orderId &&
                item.UserId == userId &&
                item.Order != null &&
                item.Order.BuyerId == userId)
            ?? throw new NotFoundException("Không tìm thấy giao dịch thanh toán.");

        EnsurePayPal(payment);
        return ApiResponse<PayPalPaymentDto>.Ok(ToDto(payment));
    }

    public async Task<ApiResponse<PayPalPaymentDto>> SimulatePayPalAsync(
        int userId,
        int orderId,
        SimulatePayPalRequestDto request)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        var payment = await PaymentQuery(asTracking: true)
            .SingleOrDefaultAsync(item =>
                item.OrderId == orderId &&
                item.UserId == userId &&
                item.Order != null &&
                item.Order.BuyerId == userId)
            ?? throw new NotFoundException("Không tìm thấy giao dịch thanh toán.");

        EnsurePayPal(payment);
        var requestedStatus = request.Succeeded ? "Paid" : "Failed";
        if (!string.Equals(payment.Status, "Pending", StringComparison.OrdinalIgnoreCase))
        {
            if (!string.Equals(payment.Status, requestedStatus, StringComparison.OrdinalIgnoreCase))
            {
                throw new BadRequestException("Giao dịch đã được xử lý và không thể thay đổi kết quả.");
            }

            await transaction.CommitAsync();
            return ApiResponse<PayPalPaymentDto>.Ok(
                ToDto(payment),
                request.Succeeded ? "Giao dịch đã được thanh toán trước đó." : "Giao dịch đã được huỷ trước đó.");
        }

        var order = payment.Order
            ?? throw new BadRequestException("Giao dịch không liên kết với đơn hàng hợp lệ.");

        if (order.BuyerId != userId)
        {
            throw new NotFoundException("Không tìm thấy giao dịch thanh toán.");
        }

        if (request.Succeeded)
        {
            payment.Status = "Paid";
            payment.PaidAt = DateTime.UtcNow;
            order.Status = "Confirmed";
        }
        else
        {
            payment.Status = "Failed";
            payment.PaidAt = null;
            order.Status = "Cancelled";
            await CompensateCancelledOrderAsync(userId, order);
        }

        await context.SaveChangesAsync();
        await transaction.CommitAsync();

        return ApiResponse<PayPalPaymentDto>.Ok(
            ToDto(payment),
            request.Succeeded
                ? "Thanh toán PayPal mô phỏng thành công."
                : "Thanh toán PayPal mô phỏng thất bại. Đơn hàng đã huỷ, tồn kho và giỏ hàng đã được khôi phục.");
    }

    private IQueryable<Payment> PaymentQuery(bool asTracking)
    {
        var query = context.Payments
            .Include(item => item.Order)
                .ThenInclude(order => order!.OrderItems)
                    .ThenInclude(orderItem => orderItem.Product)
            .Include(item => item.Order)
                .ThenInclude(order => order!.ShippingInfos)
            .AsSplitQuery()
            .AsQueryable();

        return asTracking ? query : query.AsNoTracking();
    }

    private async Task CompensateCancelledOrderAsync(int userId, OrderTable order)
    {
        var now = DateTime.UtcNow;
        var isAuctionOrder = order.OrderItems.Any(item => item.Product?.IsAuction == true);

        foreach (var shippingInfo in order.ShippingInfos)
        {
            shippingInfo.Status = "Cancelled";
        }

        foreach (var orderItem in order.OrderItems)
        {
            if (!orderItem.ProductId.HasValue || !orderItem.Quantity.HasValue || orderItem.Quantity <= 0)
            {
                continue;
            }

            var productId = orderItem.ProductId.Value;
            var quantity = orderItem.Quantity.Value;
            var inventory = await context.Inventories
                .SingleOrDefaultAsync(item => item.ProductId == productId)
                ?? throw new BadRequestException($"Không tìm thấy tồn kho của sản phẩm #{productId} để hoàn tác giao dịch.");

            inventory.Quantity = (inventory.Quantity ?? 0) + quantity;
            inventory.LastUpdated = now;

            if (isAuctionOrder)
            {
                continue;
            }

            var cartItem = await context.CartItems
                .SingleOrDefaultAsync(item => item.UserId == userId && item.ProductId == productId);

            if (cartItem is null)
            {
                context.CartItems.Add(new CartItem
                {
                    UserId = userId,
                    ProductId = productId,
                    Quantity = quantity,
                    CreatedAt = now,
                    UpdatedAt = now
                });
            }
            else
            {
                if (cartItem.Quantity + quantity > 99)
                {
                    throw new BadRequestException($"Không thể khôi phục sản phẩm #{productId} vì số lượng trong giỏ vượt quá 99.");
                }

                cartItem.Quantity += quantity;
                cartItem.UpdatedAt = now;
            }
        }
    }

    private static void EnsurePayPal(Payment payment)
    {
        if (!string.Equals(payment.Method, "PayPal", StringComparison.OrdinalIgnoreCase))
        {
            throw new BadRequestException("Đơn hàng này không sử dụng phương thức PayPal.");
        }
    }

    private static PayPalPaymentDto ToDto(Payment payment) => new()
    {
        OrderId = payment.OrderId ?? 0,
        ItemCount = payment.Order?.OrderItems.Sum(item => item.Quantity ?? 0) ?? 0,
        Amount = payment.Amount ?? 0,
        Method = payment.Method ?? "PayPal",
        PaymentStatus = payment.Status ?? "Pending",
        OrderStatus = payment.Order?.Status ?? "Pending",
        PaidAt = payment.PaidAt
    };
}
