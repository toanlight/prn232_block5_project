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

        if (Token() is not null)
        {
            var favorite = await apiClient.GetAsync<bool>($"favorites/contains/{id}", Token());
            ViewBag.IsFavorite = favorite?.Success == true && favorite.Data;
        }

        if (product.Data.IsAuction)
        {
            var auction = await apiClient.GetAsync<AuctionDetailViewModel>(
                $"auctions/product/{id}",
                Token());
            if (auction?.Success == true)
            {
                product.Data.Auction = auction.Data;
            }
        }

        return View(product.Data);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> PlaceBid(int productId, decimal bidAmount)
    {
        var token = Token();
        if (token is null)
        {
            return RedirectToAction("Login", "Auth", new { returnUrl = $"/Products/Details/{productId}" });
        }

        var response = await apiClient.PostAsync<AuctionDetailViewModel>(
            $"auctions/product/{productId}/bids",
            new { BidAmount = bidAmount },
            token);

        if (response?.Success == true)
        {
            TempData["SuccessMessage"] = response.Message;
        }
        else
        {
            TempData["ErrorMessage"] = response?.Message ?? "Không thể đặt giá. Vui lòng thử lại.";
        }

        return RedirectToAction(nameof(Details), new { id = productId });
    }

    private string? Token() =>
        HttpContext.Session.GetString("AccessToken") ?? Request.Cookies["AccessToken"];
}
