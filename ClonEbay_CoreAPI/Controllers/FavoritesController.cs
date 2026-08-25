using System.Security.Claims;
using ClonEbay_CoreAPI.Common.Models;
using ClonEbay_CoreAPI.DTOs.Commerce;
using ClonEbay_CoreAPI.Exceptions;
using ClonEbay_CoreAPI.Models;
using ClonEbay_CoreAPI.Services.Implementations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClonEbay_CoreAPI.Controllers;

[ApiController, Authorize]
[Route("api/[controller]")]
public sealed class FavoritesController(CloneEbayDbContext context) : ControllerBase
{
    private int UserId() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
        ? id : throw new UnauthorizedException();

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var items = await context.FavoriteProducts.AsNoTracking()
            .Where(x => x.UserId == UserId())
            .Include(x => x.Product).ThenInclude(x => x.Category)
            .Include(x => x.Product).ThenInclude(x => x.Reviews)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return Ok(ApiResponse<IReadOnlyList<ProductListItemDto>>.Ok(items.Select(x => ToDto(x.Product)).ToList()));
    }

    [HttpGet("contains/{productId:int}")]
    public async Task<IActionResult> Contains(int productId) => Ok(ApiResponse<bool>.Ok(
        await context.FavoriteProducts.AsNoTracking().AnyAsync(x => x.UserId == UserId() && x.ProductId == productId)));

    [HttpPost("items/{productId:int}")]
    public async Task<IActionResult> Add(int productId)
    {
        var userId = UserId();
        if (!await context.Products.AnyAsync(x => x.Id == productId)) throw new NotFoundException("Sản phẩm", productId);
        if (!await context.FavoriteProducts.AnyAsync(x => x.UserId == userId && x.ProductId == productId))
        {
            context.FavoriteProducts.Add(new FavoriteProduct { UserId = userId, ProductId = productId, CreatedAt = DateTime.UtcNow });
            await context.SaveChangesAsync();
        }
        return Ok(ApiResponse<bool>.Ok(true, "Đã thêm vào mục ưa thích."));
    }

    [HttpDelete("items/{productId:int}")]
    public async Task<IActionResult> Remove(int productId)
    {
        var item = await context.FavoriteProducts.FirstOrDefaultAsync(x => x.UserId == UserId() && x.ProductId == productId);
        if (item is not null)
        {
            context.FavoriteProducts.Remove(item);
            await context.SaveChangesAsync();
        }
        return Ok(ApiResponse<bool>.Ok(false, "Đã bỏ khỏi mục ưa thích."));
    }

    private static ProductListItemDto ToDto(Product product) => new()
    {
        Id = product.Id,
        Title = product.Title ?? "Sản phẩm",
        Price = product.Price ?? 0,
        ImageUrl = ProductService.FirstImage(product.Images),
        CategoryName = product.Category?.Name,
        IsAuction = product.IsAuction ?? false,
        AverageRating = product.Reviews.Count == 0 ? 0 : (decimal)Math.Round(product.Reviews.Average(x => x.Rating ?? 0), 1),
        ReviewCount = product.Reviews.Count
    };
}
