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
    private enum OrderProgressStage
    {
        Pending = 1,
        Confirmed = 2,
        Shipping = 3,
        Delivered = 4,
        Cancelled = 5
    }

    public async Task<ApiResponse<OrderHistoryPageDto>> GetOrderHistoryAsync(
        int userId,
        int page = 1,
        int pageSize = 10,
        string? status = null)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);
        var statusFilter = NormalizeStatusFilter(status);

        var query = context.OrderTables
            .AsNoTracking()
            .Where(order => order.BuyerId == userId);

        query = ApplyStatusFilter(query, statusFilter);
        var totalItems = await query.CountAsync();
        var orders = await query
            .OrderByDescending(order => order.OrderDate)
            .ThenByDescending(order => order.Id)
            .Include(order => order.OrderItems)
                .ThenInclude(item => item.Product)
            .Include(order => order.Payments)
            .Include(order => order.ShippingInfos)
            .AsSplitQuery()
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var result = new OrderHistoryPageDto
        {
            Items = orders.Select(ToHistoryItem).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize),
            StatusFilter = statusFilter
        };

        return ApiResponse<OrderHistoryPageDto>.Ok(result);
    }

    public async Task<ApiResponse<OrderDetailDto>> GetOrderDetailAsync(int userId, int orderId)
    {
        var order = await context.OrderTables
            .AsNoTracking()
            .Where(item => item.Id == orderId && item.BuyerId == userId)
            .Include(item => item.Address)
            .Include(item => item.OrderItems)
                .ThenInclude(item => item.Product)
            .Include(item => item.Payments)
            .Include(item => item.ShippingInfos)
            .AsSplitQuery()
            .SingleOrDefaultAsync()
            ?? throw new NotFoundException("Không tìm thấy đơn hàng.");

        return ApiResponse<OrderDetailDto>.Ok(ToOrderDetail(order));
    }

    public async Task<ApiResponse<CheckoutDto>> GetCheckoutAsync(int userId, int? addressId = null)
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

        var checkout = new CheckoutDto
        {
            Items = ToCartDtos(cartItems),
            Addresses = addresses.Select(ToCheckoutAddress).ToList(),
            SelectedAddressId = selectedAddress?.Id,
            ShippingFee = selectedAddress is null ? 0 : CalculateShippingFee(selectedAddress)
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
        var total = subtotal + shippingFee;
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

    private static OrderHistoryItemDto ToHistoryItem(OrderTable order)
    {
        var payment = order.Payments.OrderByDescending(item => item.Id).FirstOrDefault();
        var shipping = order.ShippingInfos.OrderByDescending(item => item.Id).FirstOrDefault();
        var stage = ResolveStage(order.Status, shipping?.Status);

        return new OrderHistoryItemDto
        {
            OrderId = order.Id,
            OrderDate = order.OrderDate,
            ItemCount = order.OrderItems.Sum(item => item.Quantity ?? 0),
            Total = order.TotalPrice ?? 0,
            Status = order.Status ?? "Pending",
            StatusKey = StageKey(stage),
            StatusLabel = StageLabel(stage),
            PaymentMethod = payment?.Method,
            PaymentStatus = payment?.Status,
            ShippingStatus = shipping?.Status,
            PreviewItems = order.OrderItems
                .OrderBy(item => item.Id)
                .Take(3)
                .Select(item => new OrderHistoryPreviewItemDto
                {
                    ProductId = item.ProductId ?? 0,
                    Title = item.Product?.Title ?? "Sản phẩm không còn tồn tại",
                    ImageUrl = ProductService.FirstImage(item.Product?.Images),
                    Quantity = item.Quantity ?? 0
                })
                .ToList()
        };
    }

    private static OrderDetailDto ToOrderDetail(OrderTable order)
    {
        var payment = order.Payments.OrderByDescending(item => item.Id).FirstOrDefault();
        var shipping = order.ShippingInfos.OrderByDescending(item => item.Id).FirstOrDefault();
        var stage = ResolveStage(order.Status, shipping?.Status);
        var subtotal = order.OrderItems.Sum(item => (item.UnitPrice ?? 0) * (item.Quantity ?? 0));
        var total = order.TotalPrice ?? subtotal;

        return new OrderDetailDto
        {
            OrderId = order.Id,
            OrderDate = order.OrderDate,
            ItemCount = order.OrderItems.Sum(item => item.Quantity ?? 0),
            Subtotal = subtotal,
            ShippingFee = Math.Max(0, total - subtotal),
            Total = total,
            Status = order.Status ?? "Pending",
            StatusKey = StageKey(stage),
            StatusLabel = StageLabel(stage),
            CanReview = stage == OrderProgressStage.Delivered,
            CanRequestReturn = stage == OrderProgressStage.Delivered,
            Address = order.Address is null ? null : new OrderHistoryAddressDto
            {
                FullName = order.Address.FullName ?? string.Empty,
                Phone = order.Address.Phone ?? string.Empty,
                Street = order.Address.Street ?? string.Empty,
                City = order.Address.City ?? string.Empty,
                State = order.Address.State,
                Country = order.Address.Country ?? string.Empty,
                PostalCode = order.Address.PostalCode
            },
            Payment = payment is null ? null : new OrderHistoryPaymentDto
            {
                Method = payment.Method ?? string.Empty,
                Status = payment.Status ?? string.Empty,
                Amount = payment.Amount ?? total,
                PaidAt = payment.PaidAt
            },
            Shipping = shipping is null ? null : new OrderHistoryShippingDto
            {
                Carrier = shipping.Carrier ?? string.Empty,
                TrackingNumber = shipping.TrackingNumber,
                Status = shipping.Status ?? string.Empty,
                EstimatedArrival = shipping.EstimatedArrival
            },
            Items = order.OrderItems
                .OrderBy(item => item.Id)
                .Select(item => new OrderDetailItemDto
                {
                    ProductId = item.ProductId ?? 0,
                    Title = item.Product?.Title ?? "Sản phẩm không còn tồn tại",
                    ImageUrl = ProductService.FirstImage(item.Product?.Images),
                    UnitPrice = item.UnitPrice ?? 0,
                    Quantity = item.Quantity ?? 0
                })
                .ToList(),
            Timeline = BuildTimeline(stage, order.OrderDate, payment?.PaidAt)
        };
    }

    private static IReadOnlyList<OrderTimelineStepDto> BuildTimeline(
        OrderProgressStage stage,
        DateTime? orderDate,
        DateTime? paidAt)
    {
        if (stage == OrderProgressStage.Cancelled)
        {
            return
            [
                new OrderTimelineStepDto
                {
                    Code = "placed",
                    Label = "Đã đặt hàng",
                    Description = "Đơn hàng đã được tạo trên hệ thống.",
                    Timestamp = orderDate,
                    IsCompleted = true
                },
                new OrderTimelineStepDto
                {
                    Code = "cancelled",
                    Label = "Đã huỷ",
                    Description = "Đơn hàng đã bị huỷ hoặc thanh toán không thành công.",
                    IsCurrent = true,
                    IsCancelled = true
                }
            ];
        }

        var definitions = new[]
        {
            (Code: "placed", Label: "Đã đặt hàng", Description: "Đơn hàng đã được tạo trên hệ thống.", Timestamp: orderDate),
            (Code: "pending", Label: "Chờ xác nhận", Description: "Đơn hàng đang chờ xác nhận.", Timestamp: orderDate),
            (Code: "confirmed", Label: "Đã xác nhận", Description: "Đơn hàng đã được xác nhận và đang chuẩn bị giao.", Timestamp: paidAt),
            (Code: "shipping", Label: "Đang giao", Description: "Đơn hàng đang trên đường giao đến bạn.", Timestamp: (DateTime?)null),
            (Code: "delivered", Label: "Đã giao", Description: "Đơn hàng đã được giao thành công.", Timestamp: (DateTime?)null)
        };
        var currentIndex = (int)stage;

        return definitions.Select((item, index) => new OrderTimelineStepDto
        {
            Code = item.Code,
            Label = item.Label,
            Description = item.Description,
            Timestamp = item.Timestamp,
            IsCompleted = index < currentIndex,
            IsCurrent = index == currentIndex
        }).ToList();
    }

    private static IQueryable<OrderTable> ApplyStatusFilter(
        IQueryable<OrderTable> query,
        string statusFilter) => statusFilter switch
        {
            "pending" => query.Where(order => order.Status == null || order.Status == "Pending"),
            "confirmed" => query.Where(order => order.Status == "Confirmed" || order.Status == "Processing"),
            "shipping" => query.Where(order =>
                order.Status == "Shipped" ||
                order.Status == "Shipping" ||
                order.ShippingInfos.Any(info =>
                    info.Status == "In_Transit" ||
                    info.Status == "In Transit" ||
                    info.Status == "Shipping" ||
                    info.Status == "Shipped")),
            "delivered" => query.Where(order =>
                order.Status == "Delivered" ||
                order.Status == "Completed" ||
                order.ShippingInfos.Any(info => info.Status == "Delivered")),
            "cancelled" => query.Where(order =>
                order.Status == "Cancelled" ||
                order.Status == "Canceled" ||
                order.Status == "Failed"),
            _ => query
        };

    private static string NormalizeStatusFilter(string? status)
    {
        var normalized = status?.Trim().ToLowerInvariant();
        return normalized is "pending" or "confirmed" or "shipping" or "delivered" or "cancelled"
            ? normalized
            : "all";
    }

    private static OrderProgressStage ResolveStage(string? orderStatus, string? shippingStatus)
    {
        var order = NormalizeStatusToken(orderStatus);
        var shipping = NormalizeStatusToken(shippingStatus);

        if (order is "CANCELLED" or "CANCELED" or "FAILED" || shipping is "CANCELLED" or "CANCELED" or "FAILED")
        {
            return OrderProgressStage.Cancelled;
        }

        if (order is "DELIVERED" or "COMPLETED" || shipping is "DELIVERED" or "COMPLETED")
        {
            return OrderProgressStage.Delivered;
        }

        if (order is "SHIPPED" or "SHIPPING" or "INTRANSIT" || shipping is "SHIPPED" or "SHIPPING" or "INTRANSIT" or "DELIVERING")
        {
            return OrderProgressStage.Shipping;
        }

        if (order is "CONFIRMED" or "PROCESSING")
        {
            return OrderProgressStage.Confirmed;
        }

        return OrderProgressStage.Pending;
    }

    private static string NormalizeStatusToken(string? status) => string.IsNullOrWhiteSpace(status)
        ? string.Empty
        : status.Trim()
            .Replace("_", string.Empty)
            .Replace("-", string.Empty)
            .Replace(" ", string.Empty)
            .ToUpperInvariant();

    private static string StageKey(OrderProgressStage stage) => stage switch
    {
        OrderProgressStage.Confirmed => "confirmed",
        OrderProgressStage.Shipping => "shipping",
        OrderProgressStage.Delivered => "delivered",
        OrderProgressStage.Cancelled => "cancelled",
        _ => "pending"
    };

    private static string StageLabel(OrderProgressStage stage) => stage switch
    {
        OrderProgressStage.Confirmed => "Đã xác nhận",
        OrderProgressStage.Shipping => "Đang giao",
        OrderProgressStage.Delivered => "Đã giao",
        OrderProgressStage.Cancelled => "Đã huỷ",
        _ => "Chờ xác nhận"
    };

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

    private static bool IsHoChiMinhCity(string? city)
    {
        if (string.IsNullOrWhiteSpace(city)) return false;
        var normalized = city.Trim().ToLowerInvariant();
        return normalized.Contains("hồ chí minh") || normalized.Contains("ho chi minh") || normalized.Contains("hcm");
    }
}
