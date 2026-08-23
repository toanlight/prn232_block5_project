using System.ComponentModel.DataAnnotations;

namespace ClonEbay_CoreAPI.DTOs.Commerce;

public sealed class PagedResultDto<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalItems { get; init; }
    public int TotalPages => (int)Math.Ceiling(TotalItems / (double)PageSize);
}

public sealed class CategoryDto { public int Id { get; init; } public string Name { get; init; } = string.Empty; }

public class ProductListItemDto
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string? ImageUrl { get; init; }
    public string? CategoryName { get; init; }
    public bool IsAuction { get; init; }
    public decimal AverageRating { get; init; }
    public int ReviewCount { get; init; }
}

public sealed class ProductDetailDto : ProductListItemDto
{
    public string? Description { get; init; }
    public DateTime? AuctionEndTime { get; init; }
    public SellerDto? Seller { get; init; }
    public IReadOnlyList<ReviewDto> Reviews { get; init; } = [];
}
public sealed class SellerDto { public int Id { get; init; } public string Name { get; init; } = string.Empty; public string? AvatarUrl { get; init; } }
public sealed class ReviewDto { public int Id { get; init; } public int Rating { get; init; } public string? Comment { get; init; } public string ReviewerName { get; init; } = "Người dùng"; public DateTime? CreatedAt { get; init; } }

public sealed class AddCartItemRequestDto { [Range(1, int.MaxValue)] public int ProductId { get; init; } [Range(1, 99)] public int Quantity { get; init; } = 1; }
public sealed class UpdateCartItemRequestDto { [Range(1, 99)] public int Quantity { get; init; } }
public sealed class CartItemDto { public int ProductId { get; init; } public string Title { get; init; } = string.Empty; public string? ImageUrl { get; init; } public decimal UnitPrice { get; init; } public int Quantity { get; init; } public decimal LineTotal => UnitPrice * Quantity; }
public sealed class CartDto { public IReadOnlyList<CartItemDto> Items { get; init; } = []; public decimal Total => Items.Sum(x => x.LineTotal); public int ItemCount => Items.Sum(x => x.Quantity); }
