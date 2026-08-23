using ClonEbay_CoreAPI.Common.Models;
using ClonEbay_CoreAPI.DTOs.Commerce;
using ClonEbay_CoreAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ClonEbay_CoreAPI.Controllers;
[ApiController]
[Route("api/[controller]")]
public sealed class ProductsController(IProductService productService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? search, [FromQuery] int? categoryId, [FromQuery] decimal? minPrice, [FromQuery] decimal? maxPrice, [FromQuery] int page = 1, [FromQuery] int pageSize = 12)
    {
        if (minPrice < 0 || maxPrice < 0 || (minPrice.HasValue && maxPrice.HasValue && minPrice > maxPrice)) return BadRequest(ApiResponse<object>.Fail("Khoảng giá không hợp lệ."));
        return Ok(await productService.GetProductsAsync(search, categoryId, minPrice, maxPrice, page, pageSize));
    }
    [HttpGet("categories")] public async Task<IActionResult> Categories() => Ok(await productService.GetCategoriesAsync());
    [HttpGet("{id:int}")] public async Task<IActionResult> GetById(int id) => Ok(await productService.GetProductAsync(id));
}
