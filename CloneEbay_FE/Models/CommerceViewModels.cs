using System.ComponentModel.DataAnnotations;
namespace CloneEbay_FE.Models;
public sealed class CategoryViewModel { public int Id { get; set; } public string Name { get; set; } = string.Empty; }
public class ProductCardViewModel { public int Id { get; set; } public string Title { get; set; } = string.Empty; public decimal Price { get; set; } public string? ImageUrl { get; set; } public string? CategoryName { get; set; } public bool IsAuction { get; set; } public decimal AverageRating { get; set; } public int ReviewCount { get; set; } }
public sealed class ProductDetailViewModel : ProductCardViewModel { public string? Description { get; set; } public DateTime? AuctionEndTime { get; set; } public SellerViewModel? Seller { get; set; } public List<ProductDetailReviewViewModel> Reviews { get; set; } = []; }
public sealed class SellerViewModel { public int Id { get; set; } public string Name { get; set; } = string.Empty; public string? AvatarUrl { get; set; } }
public sealed class ProductDetailReviewViewModel { public int Id { get; set; } public int Rating { get; set; } public string? Comment { get; set; } public string ReviewerName { get; set; } = string.Empty; public DateTime? CreatedAt { get; set; } }
public sealed class PagedProductViewModel { public List<ProductCardViewModel> Items { get; set; } = []; public int Page { get; set; } public int PageSize { get; set; } public int TotalItems { get; set; } public int TotalPages { get; set; } }
public sealed class ProductIndexViewModel { public PagedProductViewModel Results { get; set; } = new(); public List<CategoryViewModel> Categories { get; set; } = []; public string? Search { get; set; } public int? CategoryId { get; set; } public decimal? MinPrice { get; set; } public decimal? MaxPrice { get; set; } }
public sealed class CartItemViewModel { public int ProductId { get; set; } public string Title { get; set; } = string.Empty; public string? ImageUrl { get; set; } public decimal UnitPrice { get; set; } public int Quantity { get; set; } public decimal LineTotal { get; set; } }
public sealed class CartViewModel { public List<CartItemViewModel> Items { get; set; } = []; public decimal Total { get; set; } public int ItemCount { get; set; } }
public sealed class AddCartItemViewModel { [Range(1, int.MaxValue)] public int ProductId { get; set; } [Range(1, 99)] public int Quantity { get; set; } = 1; public string Title { get; set; } = string.Empty; public string? ImageUrl { get; set; } public decimal UnitPrice { get; set; } }

public sealed class CheckoutAddressViewModel
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? State { get; set; }
    public string Country { get; set; } = string.Empty;
    public string? PostalCode { get; set; }
    public bool IsDefault { get; set; }
    public decimal ShippingFee { get; set; }
}

public sealed class CheckoutViewModel
{
    public List<CartItemViewModel> Items { get; set; } = [];
    public List<CheckoutAddressViewModel> Addresses { get; set; } = [];
    public int? SelectedAddressId { get; set; }
    public int ItemCount { get; set; }
    public decimal Subtotal { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal Total { get; set; }
}

public sealed class PlaceOrderViewModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn địa chỉ giao hàng")]
    public int AddressId { get; set; }

    [Required]
    [RegularExpression("^(COD|PayPal)$", ErrorMessage = "Phương thức thanh toán không hợp lệ")]
    public string PaymentMethod { get; set; } = "COD";
}

public sealed class OrderCreatedViewModel
{
    public int OrderId { get; set; }
    public DateTime OrderDate { get; set; }
    public int ItemCount { get; set; }
    public decimal Subtotal { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal Total { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
}

public sealed class PayPalPaymentViewModel
{
    public int OrderId { get; set; }
    public int ItemCount { get; set; }
    public decimal Amount { get; set; }
    public string Method { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public string OrderStatus { get; set; } = string.Empty;
    public DateTime? PaidAt { get; set; }
}

public sealed class OrderHistoryItemViewModel
{
    public int OrderId { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? AddressText { get; set; }
    public string? RecipientName { get; set; }
    public string? RecipientPhone { get; set; }
    public string? PaymentMethod { get; set; }
    public string? PaymentStatus { get; set; }
    public int ItemCount { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal Subtotal { get; set; }
    public List<OrderItemDetailViewModel> Items { get; set; } = [];
}

public sealed class OrderItemDetailViewModel
{
    public int ProductId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal => UnitPrice * Quantity;
}
