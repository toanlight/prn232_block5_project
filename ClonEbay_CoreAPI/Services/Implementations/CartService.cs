using ClonEbay_CoreAPI.Common.Models;
using ClonEbay_CoreAPI.DTOs.Commerce;
using ClonEbay_CoreAPI.Exceptions;
using ClonEbay_CoreAPI.Models;
using ClonEbay_CoreAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
namespace ClonEbay_CoreAPI.Services.Implementations;
public sealed class CartService(CloneEbayDbContext context) : ICartService
{
    public async Task<ApiResponse<CartDto>> GetAsync(int userId) => ApiResponse<CartDto>.Ok(await BuildAsync(userId));
    public async Task<ApiResponse<CartDto>> AddAsync(int userId, AddCartItemRequestDto request) { await AddOrIncreaseAsync(userId, request.ProductId, request.Quantity); return ApiResponse<CartDto>.Ok(await BuildAsync(userId), "Đã thêm vào giỏ hàng."); }
    public async Task<ApiResponse<CartDto>> UpdateAsync(int userId, int productId, UpdateCartItemRequestDto request) { var item = await context.CartItems.FirstOrDefaultAsync(x => x.UserId == userId && x.ProductId == productId) ?? throw new NotFoundException("Sản phẩm trong giỏ hàng"); item.Quantity = request.Quantity; item.UpdatedAt = DateTime.UtcNow; await context.SaveChangesAsync(); return ApiResponse<CartDto>.Ok(await BuildAsync(userId)); }
    public async Task<ApiResponse<CartDto>> RemoveAsync(int userId, int productId) { var item = await context.CartItems.FirstOrDefaultAsync(x => x.UserId == userId && x.ProductId == productId); if (item is not null) { context.CartItems.Remove(item); await context.SaveChangesAsync(); } return ApiResponse<CartDto>.Ok(await BuildAsync(userId)); }
    public async Task<ApiResponse<CartDto>> MergeAsync(int userId, IEnumerable<AddCartItemRequestDto> items) { foreach (var item in items.Where(x => x.ProductId > 0 && x.Quantity > 0)) await AddOrIncreaseAsync(userId, item.ProductId, Math.Min(item.Quantity, 99)); return ApiResponse<CartDto>.Ok(await BuildAsync(userId), "Đã đồng bộ giỏ hàng."); }
    private async Task AddOrIncreaseAsync(int userId, int productId, int quantity) { if (!await context.Products.AnyAsync(x => x.Id == productId)) throw new NotFoundException("Sản phẩm", productId); var item = await context.CartItems.FirstOrDefaultAsync(x => x.UserId == userId && x.ProductId == productId); if (item is null) context.CartItems.Add(new CartItem { UserId=userId, ProductId=productId, Quantity=Math.Min(quantity,99), CreatedAt=DateTime.UtcNow, UpdatedAt=DateTime.UtcNow }); else { item.Quantity=Math.Min(item.Quantity+quantity,99); item.UpdatedAt=DateTime.UtcNow; } await context.SaveChangesAsync(); }
    private async Task<CartDto> BuildAsync(int userId) { var items = await context.CartItems.AsNoTracking().Where(x => x.UserId == userId).Include(x => x.Product).OrderByDescending(x => x.UpdatedAt).ToListAsync(); return new CartDto { Items = items.Select(x => new CartItemDto { ProductId=x.ProductId, Quantity=x.Quantity, Title=x.Product.Title ?? "Sản phẩm", UnitPrice=x.Product.Price ?? 0, ImageUrl=ProductService.FirstImage(x.Product.Images) }).ToList() }; }
}
