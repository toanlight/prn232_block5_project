using CloneEbay_FE.Models;
using CloneEbay_FE.Services;
using Microsoft.AspNetCore.Mvc;
namespace CloneEbay_FE.Controllers;
public sealed class ProductsController(IApiClientService apiClient) : Controller
{
    public async Task<IActionResult> Index(string? search, int? categoryId, decimal? minPrice, decimal? maxPrice, int page = 1)
    {
        var query = $"products?search={Uri.EscapeDataString(search ?? string.Empty)}&categoryId={categoryId}&minPrice={minPrice}&maxPrice={maxPrice}&page={page}&pageSize=12";
        var products = await apiClient.GetAsync<PagedProductViewModel>(query);
        var categories = await apiClient.GetAsync<List<CategoryViewModel>>("products/categories");
        return View(new ProductIndexViewModel { Results = products?.Data ?? new(), Categories = categories?.Data ?? [], Search=search, CategoryId=categoryId, MinPrice=minPrice, MaxPrice=maxPrice });
    }
    public async Task<IActionResult> Details(int id)
    {
        var product = await apiClient.GetAsync<ProductDetailViewModel>($"products/{id}");
        if (product?.Success != true || product.Data is null) return NotFound();
        return View(product.Data);
    }
}
