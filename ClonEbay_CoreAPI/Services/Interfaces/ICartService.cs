using ClonEbay_CoreAPI.Common.Models;
using ClonEbay_CoreAPI.DTOs.Commerce;
namespace ClonEbay_CoreAPI.Services.Interfaces;
public interface ICartService
{
    Task<ApiResponse<CartDto>> GetAsync(int userId);
    Task<ApiResponse<CartDto>> AddAsync(int userId, AddCartItemRequestDto request);
    Task<ApiResponse<CartDto>> UpdateAsync(int userId, int productId, UpdateCartItemRequestDto request);
    Task<ApiResponse<CartDto>> RemoveAsync(int userId, int productId);
    Task<ApiResponse<CartDto>> MergeAsync(int userId, IEnumerable<AddCartItemRequestDto> items);
}
