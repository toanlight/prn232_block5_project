using ClonEbay_CoreAPI.Common.Models;
using ClonEbay_CoreAPI.DTOs.Commerce;
using ClonEbay_CoreAPI.Exceptions;
using ClonEbay_CoreAPI.Models;
using ClonEbay_CoreAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClonEbay_CoreAPI.Services.Implementations;
public sealed class ProductService(CloneEbayDbContext context) : IProductService
{
    public async Task<ApiResponse<PagedResultDto<ProductListItemDto>>> GetProductsAsync(string? search, int? categoryId, decimal? minPrice, decimal? maxPrice, int page, int pageSize)
    {
        page = Math.Max(page, 1); pageSize = Math.Clamp(pageSize, 1, 48);
        var query = context.Products.AsNoTracking().AsSplitQuery().Include(x => x.Category).Include(x => x.Reviews).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search)) { var term = search.Trim(); query = query.Where(x => x.Title != null && x.Title.Contains(term)); }
        if (categoryId.HasValue) query = query.Where(x => x.CategoryId == categoryId);
        if (minPrice.HasValue) query = query.Where(x => x.Price >= minPrice);
        if (maxPrice.HasValue) query = query.Where(x => x.Price <= maxPrice);
        var total = await query.CountAsync();
        var products = await query.OrderByDescending(x => x.Id).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return ApiResponse<PagedResultDto<ProductListItemDto>>.Ok(new PagedResultDto<ProductListItemDto> { Items = products.Select(ToListDto).ToList(), Page = page, PageSize = pageSize, TotalItems = total });
    }
    public async Task<ApiResponse<ProductDetailDto>> GetProductAsync(int id)
    {
        var product = await context.Products.AsNoTracking().AsSplitQuery().Include(x => x.Category).Include(x => x.Seller).Include(x => x.Reviews).ThenInclude(x => x.Reviewer).FirstOrDefaultAsync(x => x.Id == id);
        if (product is null) throw new NotFoundException("Sản phẩm", id);
        var baseDto = ToListDto(product);
        return ApiResponse<ProductDetailDto>.Ok(new ProductDetailDto { Id = baseDto.Id, Title = baseDto.Title, Price = baseDto.Price, ImageUrl = baseDto.ImageUrl, CategoryName = baseDto.CategoryName, IsAuction = baseDto.IsAuction, AverageRating = baseDto.AverageRating, ReviewCount = baseDto.ReviewCount, Description = product.Description, AuctionEndTime = product.AuctionEndTime, Seller = product.Seller is null ? null : new SellerDto { Id = product.Seller.Id, Name = product.Seller.FullName ?? product.Seller.Username ?? "Người bán", AvatarUrl = product.Seller.AvatarUrl }, Reviews = product.Reviews.OrderByDescending(x => x.CreatedAt).Select(x => new ReviewDto { Id=x.Id, Rating=x.Rating ?? 0, Comment=x.Comment, CreatedAt=x.CreatedAt, ReviewerName=x.Reviewer?.FullName ?? x.Reviewer?.Username ?? "Người dùng" }).ToList() });
    }
    public async Task<ApiResponse<IReadOnlyList<CategoryDto>>> GetCategoriesAsync() => ApiResponse<IReadOnlyList<CategoryDto>>.Ok(await context.Categories.AsNoTracking().OrderBy(x => x.Name).Select(x => new CategoryDto { Id = x.Id, Name = x.Name ?? "Chưa phân loại" }).ToListAsync());
    private static ProductListItemDto ToListDto(Product x) => new() { Id=x.Id, Title=x.Title ?? "Sản phẩm", Price=x.Price ?? 0, ImageUrl=FirstImage(x.Images), CategoryName=x.Category?.Name, IsAuction=x.IsAuction ?? false, AverageRating=x.Reviews.Count == 0 ? 0 : (decimal)Math.Round(x.Reviews.Average(r => r.Rating ?? 0), 1), ReviewCount=x.Reviews.Count };
    public static string? FirstImage(string? images) => string.IsNullOrWhiteSpace(images) ? null : images.Split([',', ';', '|'], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
}
