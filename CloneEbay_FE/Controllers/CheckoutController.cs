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

        var appliedCoupons = GetAppliedCouponsFromSession();
        var couponsJson = System.Text.Json.JsonSerializer.Serialize(appliedCoupons);
        var endpoint = addressId.HasValue
            ? $"orders/checkout?addressId={addressId.Value}&coupons={Uri.EscapeDataString(couponsJson)}"
            : $"orders/checkout?coupons={Uri.EscapeDataString(couponsJson)}";

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

        model.AppliedCoupons = GetAppliedCouponsFromSession();

        var response = await apiClient.PostAsync<OrderCreatedViewModel>("orders/checkout", model, token);
        if (response?.Success != true || response.Data is null)
        {
            TempData["ErrorMessage"] = response?.Message ?? "Không thể tạo đơn hàng.";
            return RedirectToAction(nameof(Index), new { addressId = model.AddressId });
        }

        // Xoá Session Coupon sau khi tạo đơn thành công
        HttpContext.Session.Remove("AppliedCoupons");

        TempData["OrderId"] = response.Data.OrderId;
        TempData["OrderTotal"] = response.Data.Total.ToString(CultureInfo.InvariantCulture);
        TempData["PaymentMethod"] = response.Data.PaymentMethod;
        TempData["PaymentStatus"] = response.Data.PaymentStatus;
        TempData["OrderStatus"] = response.Data.Status;

        if (response.Data.PaymentMethod.Equals("PayPal", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction("PayPal", "Payment", new { orderId = response.Data.OrderId });
        }

        TempData["SuccessMessage"] = response.Message;
        return RedirectToAction(nameof(Success));
    }

    private Dictionary<int, string> GetAppliedCouponsFromSession()
    {
        var result = new Dictionary<int, string>();

        var sessionData = HttpContext.Session.GetString("AppliedCoupons");
        if (!string.IsNullOrEmpty(sessionData))
        {
            try
            {
                var dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<int, string>>(sessionData);
                if (dict != null)
                {
                    foreach (var kvp in dict) result[kvp.Key] = kvp.Value;
                }
            }
            catch { }
        }

        foreach (var key in HttpContext.Session.Keys)
        {
            if (key.StartsWith("AppliedCouponCode_"))
            {
                var productIdStr = key.Replace("AppliedCouponCode_", "");
                if (int.TryParse(productIdStr, out var pid))
                {
                    var code = HttpContext.Session.GetString(key);
                    if (!string.IsNullOrWhiteSpace(code))
                    {
                        result[pid] = code;
                    }
                }
            }
        }

        return result;
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
            PaymentMethod = TempData["PaymentMethod"]?.ToString() ?? "COD",
            PaymentStatus = TempData["PaymentStatus"]?.ToString() ?? "Pending",
            Status = TempData["OrderStatus"]?.ToString() ?? "Confirmed"
        });
    }

    private string? Token() => HttpContext.Session.GetString("AccessToken") ?? Request.Cookies["AccessToken"];

    private IActionResult RedirectToLogin() =>
        RedirectToAction("Login", "Auth", new { returnUrl = "/Checkout" });
}
