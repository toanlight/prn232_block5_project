using System.Globalization;
using CloneEbay_FE.Models;
using CloneEbay_FE.Services;
using Microsoft.AspNetCore.Mvc;

namespace CloneEbay_FE.Controllers;

public sealed class CheckoutController(IApiClientService apiClient) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(int? addressId = null)
    {
        var token = Token();
        if (token is null) return RedirectToLogin();

        var endpoint = addressId.HasValue ? $"orders/checkout?addressId={addressId.Value}" : "orders/checkout";
        var response = await apiClient.GetAsync<CheckoutViewModel>(endpoint, token);
        if (response?.Success != true || response.Data is null)
        {
            TempData["ErrorMessage"] = response?.Message ?? "Không thể tải thông tin checkout.";
            return RedirectToAction("Index", "Cart");
        }

        if (response.Data.Items.Count == 0)
        {
            TempData["WarningMessage"] = "Giỏ hàng đang trống.";
            return RedirectToAction("Index", "Cart");
        }

        return View(response.Data);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> PlaceOrder(PlaceOrderViewModel model)
    {
        var token = Token();
        if (token is null) return RedirectToLogin();

        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = ModelState.Values
                .SelectMany(value => value.Errors)
                .Select(error => error.ErrorMessage)
                .FirstOrDefault() ?? "Dữ liệu đặt hàng không hợp lệ.";
            return RedirectToAction(nameof(Index), new { addressId = model.AddressId > 0 ? (int?)model.AddressId : null });
        }

        var response = await apiClient.PostAsync<OrderCreatedViewModel>("orders/checkout", model, token);
        if (response?.Success != true || response.Data is null)
        {
            TempData["ErrorMessage"] = response?.Message ?? "Không thể tạo đơn hàng.";
            return RedirectToAction(nameof(Index), new { addressId = model.AddressId });
        }

        TempData["OrderId"] = response.Data.OrderId;
        TempData["OrderTotal"] = response.Data.Total.ToString(CultureInfo.InvariantCulture);
        TempData["PaymentMethod"] = response.Data.PaymentMethod;
        TempData["SuccessMessage"] = response.Message;
        return RedirectToAction(nameof(Success));
    }

    [HttpGet]
    public IActionResult Success()
    {
        if (!TempData.TryGetValue("OrderId", out var orderIdValue))
        {
            return RedirectToAction("Index", "Cart");
        }

        _ = int.TryParse(orderIdValue?.ToString(), out var orderId);
        _ = decimal.TryParse(
            TempData["OrderTotal"]?.ToString(),
            NumberStyles.Number,
            CultureInfo.InvariantCulture,
            out var total);

        return View(new OrderCreatedViewModel
        {
            OrderId = orderId,
            Total = total,
            PaymentMethod = TempData["PaymentMethod"]?.ToString() ?? "COD"
        });
    }

    private string? Token() => HttpContext.Session.GetString("AccessToken") ?? Request.Cookies["AccessToken"];

    private IActionResult RedirectToLogin() =>
        RedirectToAction("Login", "Auth", new { returnUrl = "/Checkout" });
}
