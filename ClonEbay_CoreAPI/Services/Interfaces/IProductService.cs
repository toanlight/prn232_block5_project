using ClonEbay_CoreAPI.Common.Models;
using ClonEbay_CoreAPI.DTOs.Commerce;
namespace ClonEbay_CoreAPI.Services.Interfaces;
public interface IProductService
{
    Task<ApiResponse<PagedResultDto<ProductListItemDto>>> GetProductsAsync(string? search, int? categoryId, decimal? minPrice, decimal? maxPrice, int page, int pageSize);
    Task<ApiResponse<ProductDetailDto>> GetProductAsync(int id);
    Task<ApiResponse<IReadOnlyList<CategoryDto>>> GetCategoriesAsync();
}
