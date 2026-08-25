using System.Data;
using ClonEbay_CoreAPI.Common.Models;
using ClonEbay_CoreAPI.DTOs.Commerce;
using ClonEbay_CoreAPI.DTOs.Order;
using ClonEbay_CoreAPI.Exceptions;
using ClonEbay_CoreAPI.Models;
using ClonEbay_CoreAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClonEbay_CoreAPI.Services.Implementations;

public sealed class OrderService(CloneEbayDbContext context) : IOrderService
{
    public async Task<ApiResponse<CheckoutDto>> GetCheckoutAsync(int userId, int? addressId = null, Dictionary<int, string>? appliedCoupons = null)
    {
        var cartItems = await LoadCartAsync(userId, asTracking: false);
        var addresses = await context.Addresses
            .AsNoTracking()
            .Where(address => address.UserId == userId)
            .OrderByDescending(address => address.IsDefault)
            .ThenBy(address => address.Id)
            .ToListAsync();

        var selectedAddress = addressId.HasValue
            ? addresses.FirstOrDefault(address => address.Id == addressId.Value)
            : addresses.FirstOrDefault(address => address.IsDefault) ?? addresses.FirstOrDefault();

        if (addressId.HasValue && selectedAddress is null)
        {
            throw new NotFoundException("Không tìm thấy địa chỉ giao hàng.");
        }

        var totalDiscount = await CalculateTotalDiscountAsync(cartItems, appliedCoupons);

        var checkout = new CheckoutDto
        {
            Items = ToCartDtos(cartItems),
            Addresses = addresses.Select(ToCheckoutAddress).ToList(),
            SelectedAddressId = selectedAddress?.Id,
            ShippingFee = selectedAddress is null ? 0 : CalculateShippingFee(selectedAddress),
            TotalDiscount = totalDiscount
        };

        return ApiResponse<CheckoutDto>.Ok(checkout);
    }

    public async Task<ApiResponse<OrderCreatedDto>> PlaceOrderAsync(int userId, PlaceOrderRequestDto request)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        var address = await context.Addresses
            .FirstOrDefaultAsync(item => item.Id == request.AddressId && item.UserId == userId)
            ?? throw new NotFoundException("Không tìm thấy địa chỉ giao hàng.");

        var cartItems = await LoadCartAsync(userId, asTracking: true);
        if (cartItems.Count == 0)
        {
            throw new BadRequestException("Giỏ hàng đang trống. Vui lòng thêm sản phẩm trước khi đặt hàng.");
        }

        foreach (var cartItem in cartItems)
        {
            if (cartItem.Product.IsAuction == true)
            {
                throw new BadRequestException($"Sản phẩm '{cartItem.Product.Title}' là sản phẩm đấu giá và không thể checkout trực tiếp.");
            }

            var inventory = await context.Inventories
                .AsNoTracking()
                .Where(item => item.ProductId == cartItem.ProductId)
                .OrderBy(item => item.Id)
                .FirstOrDefaultAsync();

            if (inventory is null)
            {
                throw new BadRequestException($"Sản phẩm '{cartItem.Product.Title}' chưa có thông tin tồn kho.");
            }

            var updatedRows = await context.Inventories
                .Where(item => item.Id == inventory.Id && item.Quantity >= cartItem.Quantity)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(item => item.Quantity, item => item.Quantity - cartItem.Quantity)
                    .SetProperty(item => item.LastUpdated, DateTime.UtcNow));

            if (updatedRows == 0)
            {
                var available = await context.Inventories
                    .Where(item => item.Id == inventory.Id)
                    .Select(item => item.Quantity ?? 0)
                    .SingleAsync();
                throw new BadRequestException($"Sản phẩm '{cartItem.Product.Title}' không đủ tồn kho (còn {available}, cần {cartItem.Quantity}).");
            }
        }

        var subtotal = cartItems.Sum(item => (item.Product.Price ?? 0) * item.Quantity);
        var shippingFee = CalculateShippingFee(address);
        var totalDiscount = await CalculateTotalDiscountAsync(cartItems, request.AppliedCoupons);
        var total = Math.Max(0, subtotal + shippingFee - totalDiscount);
        var now = DateTime.UtcNow;
        var paymentMethod = request.PaymentMethod.Equals("PayPal", StringComparison.OrdinalIgnoreCase)
            ? "PayPal"
            : "COD";
        var orderStatus = paymentMethod == "COD" ? "Confirmed" : "Pending";

        var order = new OrderTable
        {
            BuyerId = userId,
            AddressId = address.Id,
            OrderDate = now,
            TotalPrice = total,
            Status = orderStatus,
            OrderItems = cartItems.Select(item => new OrderItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = item.Product.Price ?? 0
            }).ToList(),
            Payments =
            [
                new Payment
                {
                    UserId = userId,
                    Amount = total,
                    Method = paymentMethod,
                    Status = "Pending"
                }
            ],
            ShippingInfos =
            [
                new ShippingInfo
                {
                    Carrier = "Standard",
                    Status = "Preparing",
                    EstimatedArrival = now.AddDays(IsHoChiMinhCity(address.City) ? 3 : 5)
                }
            ]
        };

        context.OrderTables.Add(order);
        context.CartItems.RemoveRange(cartItems);
        await context.SaveChangesAsync();
        await transaction.CommitAsync();

        var result = new OrderCreatedDto
        {
            OrderId = order.Id,
            OrderDate = now,
            ItemCount = cartItems.Sum(item => item.Quantity),
            Subtotal = subtotal,
            ShippingFee = shippingFee,
            TotalDiscount = totalDiscount,
            Total = total,
            Status = order.Status,
            PaymentMethod = paymentMethod,
            PaymentStatus = "Pending"
        };

        var message = paymentMethod == "COD"
            ? "Đặt hàng COD thành công. Bạn sẽ thanh toán khi nhận hàng."
            : "Đơn hàng đã được khởi tạo. Vui lòng hoàn tất thanh toán PayPal mô phỏng.";
        return ApiResponse<OrderCreatedDto>.Ok(result, message);
    }

    private async Task<decimal> CalculateTotalDiscountAsync(List<CartItem> cartItems, Dictionary<int, string>? appliedCoupons)
    {
        if (appliedCoupons == null || appliedCoupons.Count == 0) return 0m;

        decimal totalDiscount = 0m;
        var now = DateTime.UtcNow;

        foreach (var item in cartItems)
        {
            if (appliedCoupons.TryGetValue(item.ProductId, out var code) && !string.IsNullOrWhiteSpace(code))
            {
                var coupon = await context.Coupons
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Code == code.Trim().ToUpper() && c.ProductId == item.ProductId);

                if (coupon != null && 
                    (!coupon.StartDate.HasValue || coupon.StartDate.Value <= now) &&
                    (!coupon.EndDate.HasValue || coupon.EndDate.Value >= now) &&
                    (coupon.MaxUsage == null || coupon.MaxUsage > 0))
                {
                    var lineTotal = (item.Product.Price ?? 0) * item.Quantity;
                    var discount = lineTotal * ((coupon.DiscountPercent ?? 0m) / 100m);
                    totalDiscount += discount;
                }
            }
        }

        return Math.Round(totalDiscount, 2);
    }

    public async Task<ApiResponse<List<OrderDto>>> GetUserOrdersAsync(int userId)
    {
        var orders = await context.OrderTables
            .AsNoTracking()
            .AsSplitQuery()
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                    .ThenInclude(p => p!.Seller)
            .Include(o => o.ShippingInfos)
            .Include(o => o.Payments)
            .Where(o => o.BuyerId == userId)
            .OrderByDescending(o => o.OrderDate ?? DateTime.MinValue)
            .ToListAsync();

        var orderIds = orders.Select(order => order.Id).ToList();
        var openReturnOrderIds = await context.ReturnRequests
            .AsNoTracking()
            .Where(request =>
                request.OrderId.HasValue &&
                orderIds.Contains(request.OrderId.Value) &&
                (request.Status == "Pending" ||
                 request.Status == "Requested" ||
                 request.Status == "Escalated"))
            .Select(request => request.OrderId!.Value)
            .Distinct()
            .ToListAsync();
        var ordersWithOpenReturns = openReturnOrderIds.ToHashSet();

        var userReviews = await context.Reviews
            .AsNoTracking()
            .Where(r => r.ReviewerId == userId && r.ProductId.HasValue)
            .ToListAsync();

        var result = orders.Select(o =>
        {
            var shipping = o.ShippingInfos.FirstOrDefault();
            var payment = o.Payments.FirstOrDefault();

            return new OrderDto
            {
                Id = o.Id,
                OrderDate = o.OrderDate,
                TotalPrice = o.TotalPrice ?? 0,
                Status = o.Status ?? "Pending",
                HasPendingReturnRequest = ordersWithOpenReturns.Contains(o.Id),
                ShippingCarrier = shipping?.Carrier ?? "Standard",
                ShippingStatus = shipping?.Status ?? "Preparing",
                EstimatedArrival = shipping?.EstimatedArrival ?? o.OrderDate?.AddDays(3),
                PaymentMethod = NormalizePaymentMethod(payment?.Method),
                PaymentStatus = NormalizePaymentStatusForOrder(o.Status, payment),
                PaymentPaidAt = payment?.PaidAt,
                Items = o.OrderItems.Select(oi =>
                {
                    var review = userReviews.FirstOrDefault(r => r.ProductId == oi.ProductId);
                    return new OrderItemDto
                    {
                        Id = oi.Id,
                        ProductId = oi.ProductId ?? 0,
                        ProductTitle = oi.Product?.Title ?? "Sản phẩm",
                        ImageUrl = ProductService.FirstImage(oi.Product?.Images),
                        Quantity = oi.Quantity ?? 1,
                        UnitPrice = oi.UnitPrice ?? 0,
                        HasReviewed = review != null,
                        ReviewId = review?.Id,
                        ReviewRating = review?.Rating,
                        ReviewComment = review?.Comment,
                        ReviewDate = review?.CreatedAt,
                        SellerId = oi.Product?.SellerId,
                        SellerName = oi.Product?.Seller?.FullName ?? oi.Product?.Seller?.Username ?? "eBay Official Store"
                    };
                }).ToList()
            };
        }).ToList();

        return ApiResponse<List<OrderDto>>.Ok(result);
    }

    public async Task<ApiResponse<OrderDto>> ConfirmReceiptAsync(int userId, int orderId)
    {
        var order = await context.OrderTables
            .AsSplitQuery()
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.ShippingInfos)
            .Include(o => o.Payments)
            .SingleOrDefaultAsync(o => o.Id == orderId && o.BuyerId == userId)
            ?? throw new NotFoundException("Không tìm thấy đơn hàng.");

        var payment = order.Payments.OrderBy(item => item.Id).FirstOrDefault();
        var isCod = string.Equals(payment?.Method, "COD", StringComparison.OrdinalIgnoreCase);
        var isPaid = string.Equals(payment?.Status, "Paid", StringComparison.OrdinalIgnoreCase);
        var isDelivered = string.Equals(
            order.Status,
            nameof(Models.Enums.OrderStatus.Delivered),
            StringComparison.OrdinalIgnoreCase);
        if (isDelivered)
        {
            if (isCod && !isPaid && payment is not null)
            {
                payment.Status = "Paid";
                payment.PaidAt = DateTime.UtcNow;
                await context.SaveChangesAsync();
            }

            return ApiResponse<OrderDto>.Ok(
                ToReceiptOrderDto(order),
                "Đơn hàng đã được xác nhận nhận hàng trước đó.");
        }

        if (string.Equals(order.Status, nameof(Models.Enums.OrderStatus.Cancelled), StringComparison.OrdinalIgnoreCase) ||
            string.Equals(order.Status, nameof(Models.Enums.OrderStatus.Returned), StringComparison.OrdinalIgnoreCase))
        {
            throw new BadRequestException("Không thể xác nhận đơn hàng đã bị hủy hoặc hoàn trả.");
        }

        var canConfirmReceipt =
            string.Equals(order.Status, nameof(Models.Enums.OrderStatus.Confirmed), StringComparison.OrdinalIgnoreCase) ||
            string.Equals(order.Status, nameof(Models.Enums.OrderStatus.Shipping), StringComparison.OrdinalIgnoreCase) ||
            string.Equals(order.Status, "Shipped", StringComparison.OrdinalIgnoreCase);
        if (!canConfirmReceipt)
        {
            throw new BadRequestException("Đơn hàng chưa được thanh toán hoặc chưa sẵn sàng để xác nhận nhận hàng.");
        }

        if (payment is null)
        {
            throw new BadRequestException("Đơn hàng không có thông tin thanh toán hợp lệ.");
        }

        if (!isCod && !isPaid)
        {
            throw new BadRequestException("Đơn hàng thanh toán trực tuyến chưa được thanh toán thành công.");
        }

        var now = DateTime.UtcNow;
        if (isCod && !isPaid)
        {
            payment.Status = "Paid";
            payment.PaidAt = now;
        }

        order.Status = nameof(Models.Enums.OrderStatus.Delivered);

        foreach (var shipping in order.ShippingInfos)
        {
            shipping.Status = nameof(Models.Enums.OrderStatus.Delivered);
        }

        await context.SaveChangesAsync();

        return ApiResponse<OrderDto>.Ok(
            ToReceiptOrderDto(order),
            isCod
                ? "Xác nhận đã nhận hàng và hoàn tất thanh toán COD thành công."
                : "Xác nhận đã nhận hàng thành công. Bạn có thể viết đánh giá hoặc gửi yêu cầu hoàn trả.");
    }

    private static OrderDto ToReceiptOrderDto(OrderTable order)
    {
        var shipping = order.ShippingInfos.OrderBy(item => item.Id).FirstOrDefault();
        var payment = order.Payments.OrderBy(item => item.Id).FirstOrDefault();

        return new OrderDto
        {
            Id = order.Id,
            OrderDate = order.OrderDate,
            TotalPrice = order.TotalPrice ?? 0,
            Status = order.Status ?? "Pending",
            ShippingCarrier = shipping?.Carrier ?? "Standard",
            ShippingStatus = shipping?.Status ?? "Preparing",
            EstimatedArrival = shipping?.EstimatedArrival,
            PaymentMethod = NormalizePaymentMethod(payment?.Method),
            PaymentStatus = NormalizePaymentStatusForOrder(order.Status, payment),
            PaymentPaidAt = payment?.PaidAt,
            Items = order.OrderItems.Select(item => new OrderItemDto
            {
                Id = item.Id,
                ProductId = item.ProductId ?? 0,
                ProductTitle = item.Product?.Title ?? "Sản phẩm",
                ImageUrl = ProductService.FirstImage(item.Product?.Images),
                Quantity = item.Quantity ?? 0,
                UnitPrice = item.UnitPrice ?? 0,
                SellerId = item.Product?.SellerId
            }).ToList()
        };
    }

    private async Task<List<CartItem>> LoadCartAsync(int userId, bool asTracking)
    {
        var query = context.CartItems
            .Where(item => item.UserId == userId)
            .Include(item => item.Product)
            .OrderBy(item => item.Id)
            .AsQueryable();

        if (!asTracking) query = query.AsNoTracking();
        return await query.ToListAsync();
    }

    private static IReadOnlyList<CartItemDto> ToCartDtos(IEnumerable<CartItem> items) => items
        .Select(item => new CartItemDto
        {
            ProductId = item.ProductId,
            Title = item.Product.Title ?? "Sản phẩm",
            ImageUrl = ProductService.FirstImage(item.Product.Images),
            UnitPrice = item.Product.Price ?? 0,
            Quantity = item.Quantity
        })
        .ToList();

    private static CheckoutAddressDto ToCheckoutAddress(Address address) => new()
    {
        Id = address.Id,
        FullName = address.FullName ?? string.Empty,
        Phone = address.Phone ?? string.Empty,
        Street = address.Street ?? string.Empty,
        City = address.City ?? string.Empty,
        State = address.State,
        Country = address.Country ?? string.Empty,
        PostalCode = address.PostalCode,
        IsDefault = address.IsDefault,
        ShippingFee = CalculateShippingFee(address)
    };

    private static decimal CalculateShippingFee(Address address) => IsHoChiMinhCity(address.City) ? 30_000m : 50_000m;

    private static string NormalizePaymentMethod(string? method) =>
        string.Equals(method, "PayPal", StringComparison.OrdinalIgnoreCase)
            ? "PayPal"
            : string.Equals(method, "COD", StringComparison.OrdinalIgnoreCase)
                ? "COD"
                : string.IsNullOrWhiteSpace(method) ? "COD" : method.Trim();

    private static string NormalizePaymentStatus(string? status) =>
        string.Equals(status, "Paid", StringComparison.OrdinalIgnoreCase)
            ? "Paid"
            : string.Equals(status, "Failed", StringComparison.OrdinalIgnoreCase)
                ? "Failed"
                : string.Equals(status, "Pending", StringComparison.OrdinalIgnoreCase)
                    ? "Pending"
                    : string.IsNullOrWhiteSpace(status) ? "Pending" : status.Trim();

    private static string NormalizePaymentStatusForOrder(string? orderStatus, Payment? payment)
    {
        var codWasCollected = string.Equals(payment?.Method, "COD", StringComparison.OrdinalIgnoreCase) &&
            (string.Equals(orderStatus, "Delivered", StringComparison.OrdinalIgnoreCase) ||
             string.Equals(orderStatus, "Return Requested", StringComparison.OrdinalIgnoreCase) ||
             string.Equals(orderStatus, "Returned", StringComparison.OrdinalIgnoreCase));

        return codWasCollected ? "Paid" : NormalizePaymentStatus(payment?.Status);
    }

    private static bool IsHoChiMinhCity(string? city)
    {
        if (string.IsNullOrWhiteSpace(city)) return false;
        var normalized = city.Trim().ToLowerInvariant();
        return normalized.Contains("hồ chí minh") || normalized.Contains("ho chi minh") || normalized.Contains("hcm");
    }
}
