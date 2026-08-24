using System.ComponentModel.DataAnnotations;
using ClonEbay_CoreAPI.DTOs.Commerce;

namespace ClonEbay_CoreAPI.DTOs.Order;

public sealed class CheckoutAddressDto
{
    public int Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public string Street { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string? State { get; init; }
    public string Country { get; init; } = string.Empty;
    public string? PostalCode { get; init; }
    public bool IsDefault { get; init; }
    public decimal ShippingFee { get; init; }
}

public sealed class CheckoutDto
{
    public IReadOnlyList<CartItemDto> Items { get; init; } = [];
    public IReadOnlyList<CheckoutAddressDto> Addresses { get; init; } = [];
    public int? SelectedAddressId { get; init; }
    public int ItemCount => Items.Sum(item => item.Quantity);
    public decimal Subtotal => Items.Sum(item => item.LineTotal);
    public decimal ShippingFee { get; init; }
    public decimal TotalDiscount { get; init; }
    public decimal Total => Math.Max(0, Subtotal + ShippingFee - TotalDiscount);
}

public sealed class PlaceOrderRequestDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn địa chỉ giao hàng.")]
    public int AddressId { get; init; }

    [Required]
    [RegularExpression("^(COD|PayPal)$", ErrorMessage = "Phương thức thanh toán không hợp lệ.")]
    public string PaymentMethod { get; init; } = "COD";

    public Dictionary<int, string>? AppliedCoupons { get; init; }
}

public sealed class OrderCreatedDto
{
    public int OrderId { get; init; }
    public DateTime OrderDate { get; init; }
    public int ItemCount { get; init; }
    public decimal Subtotal { get; init; }
    public decimal ShippingFee { get; init; }
    public decimal TotalDiscount { get; init; }
    public decimal Total { get; init; }
    public string Status { get; init; } = string.Empty;
    public string PaymentMethod { get; init; } = string.Empty;
    public string PaymentStatus { get; init; } = string.Empty;
}
