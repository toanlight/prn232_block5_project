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

        var order = new OrderTable
        {
            BuyerId = userId,
            AddressId = address.Id,
            OrderDate = now,
            TotalPrice = total,
            Status = "Pending",
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

        return ApiResponse<OrderCreatedDto>.Ok(result, "Đặt hàng thành công. Giỏ hàng đã được làm trống và tồn kho đã cập nhật.");
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

    private static bool IsHoChiMinhCity(string? city)
    {
        if (string.IsNullOrWhiteSpace(city)) return false;
        var normalized = city.Trim().ToLowerInvariant();
        return normalized.Contains("hồ chí minh") || normalized.Contains("ho chi minh") || normalized.Contains("hcm");
    }
}
