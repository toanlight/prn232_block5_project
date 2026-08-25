using CloneEbay_FE.Models;
using CloneEbay_FE.Services;
using Microsoft.AspNetCore.Mvc;

namespace CloneEbay_FE.Controllers;

public sealed class FavoritesController(IApiClientService apiClient) : Controller
{
    private string? Token() => HttpContext.Session.GetString("AccessToken") ?? Request.Cookies["AccessToken"];

    public async Task<IActionResult> Index()
    {
        if (Token() is null) return RedirectToAction("Login", "Auth", new { returnUrl = "/Favorites" });
        var response = await apiClient.GetAsync<List<ProductCardViewModel>>("favorites", Token());
        return View(response?.Data ?? []);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int productId, bool isFavorite, int? returnProductId = null)
    {
        if (Token() is null) return RedirectToAction("Login", "Auth", new { returnUrl = $"/Products/Details/{productId}" });

        var response = isFavorite
            ? await apiClient.DeleteAsync<bool>($"favorites/items/{productId}", Token())
            : await apiClient.PostAsync<bool>($"favorites/items/{productId}", new { }, Token());

        TempData[response?.Success == true ? "SuccessMessage" : "ErrorMessage"] = response?.Message ?? "Không thể cập nhật mục ưa thích.";
        return returnProductId.HasValue ? RedirectToAction("Details", "Products", new { id = returnProductId }) : RedirectToAction(nameof(Index));
    }
}
