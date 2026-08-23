using CloneEbay_FE.Models;
using CloneEbay_FE.Services;
using Microsoft.AspNetCore.Mvc;
namespace CloneEbay_FE.Controllers;
public sealed class CartController(IApiClientService apiClient) : Controller
{
    private string? Token() => HttpContext.Session.GetString("AccessToken") ?? Request.Cookies["AccessToken"];
    public async Task<IActionResult> Index()
    {
        if (Token() is null) return View(new CartViewModel());
        var cart = await apiClient.GetAsync<CartViewModel>("cart", Token());
        return View(cart?.Data ?? new CartViewModel());
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(AddCartItemViewModel model, int? returnProductId = null)
    {
        if (Token() is null) return RedirectToAction("Details", "Products", new { id = model.ProductId });
        var response = await apiClient.PostAsync<CartViewModel>("cart/items", new { model.ProductId, model.Quantity }, Token());
        TempData[response?.Success == true ? "SuccessMessage" : "ErrorMessage"] = response?.Message ?? "Không thể thêm vào giỏ hàng.";
        return returnProductId.HasValue ? RedirectToAction("Details", "Products", new { id = returnProductId }) : RedirectToAction(nameof(Index));
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int productId, int quantity) { await apiClient.PutAsync<CartViewModel>($"cart/items/{productId}", new { quantity }, Token()); return RedirectToAction(nameof(Index)); }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int productId) { await apiClient.DeleteAsync<CartViewModel>($"cart/items/{productId}", Token()); return RedirectToAction(nameof(Index)); }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Sync([FromBody] List<AddCartItemViewModel> items)
    {
        if (Token() is null) return Unauthorized();
        var response = await apiClient.PostAsync<CartViewModel>("cart/merge", items.Select(x => new { x.ProductId, x.Quantity }), Token());
        return Json(new { success = response?.Success == true, message = response?.Message });
    }
}
