using System.Globalization;
using CloneEbay_FE.Models;
using CloneEbay_FE.Services;
using Microsoft.AspNetCore.Mvc;

namespace CloneEbay_FE.Controllers;

public sealed class PaymentController(IApiClientService apiClient) : Controller
{
    [HttpGet]
    public async Task<IActionResult> PayPal(int orderId)
    {
        var token = Token();
        if (token is null) return RedirectToLogin(orderId);

        var response = await apiClient.GetAsync<PayPalPaymentViewModel>($"payments/paypal/{orderId}", token);
        if (response?.Success != true || response.Data is null)
        {
            TempData["ErrorMessage"] = response?.Message ?? "Không thể tải giao dịch PayPal.";
            return RedirectToAction("Index", "Products");
        }

        if (response.Data.PaymentStatus.Equals("Paid", StringComparison.OrdinalIgnoreCase))
        {
            StoreResult(response.Data, response.Message);
            return RedirectToAction("Success", "Checkout");
        }

        if (response.Data.PaymentStatus.Equals("Failed", StringComparison.OrdinalIgnoreCase))
        {
            StoreResult(response.Data, "Giao dịch PayPal đã thất bại.");
            return RedirectToAction(nameof(Failed));
        }

        return View(response.Data);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(int orderId, bool succeeded)
    {
        var token = Token();
        if (token is null) return RedirectToLogin(orderId);

        var response = await apiClient.PostAsync<PayPalPaymentViewModel>(
            $"payments/paypal/{orderId}/simulate",
            new { Succeeded = succeeded },
            token);

        if (response?.Success != true || response.Data is null)
        {
            TempData["ErrorMessage"] = response?.Message ?? "Không thể xử lý giao dịch PayPal.";
            return RedirectToAction(nameof(PayPal), new { orderId });
        }

        StoreResult(response.Data, response.Message);
        return response.Data.PaymentStatus.Equals("Paid", StringComparison.OrdinalIgnoreCase)
            ? RedirectToAction("Success", "Checkout")
            : RedirectToAction(nameof(Failed));
    }

    [HttpGet]
    public IActionResult Failed()
    {
        if (!TempData.TryGetValue("OrderId", out var orderIdValue))
        {
            return RedirectToAction("Index", "Products");
        }

        _ = int.TryParse(orderIdValue?.ToString(), out var orderId);
        _ = decimal.TryParse(
            TempData["OrderTotal"]?.ToString(),
            NumberStyles.Number,
            CultureInfo.InvariantCulture,
            out var amount);

        return View(new PayPalPaymentViewModel
        {
            OrderId = orderId,
            Amount = amount,
            Method = "PayPal",
            PaymentStatus = "Failed",
            OrderStatus = "Cancelled"
        });
    }

    private void StoreResult(PayPalPaymentViewModel payment, string? message)
    {
        TempData["OrderId"] = payment.OrderId;
        TempData["OrderTotal"] = payment.Amount.ToString(CultureInfo.InvariantCulture);
        TempData["PaymentMethod"] = payment.Method;
        TempData["PaymentStatus"] = payment.PaymentStatus;
        TempData["OrderStatus"] = payment.OrderStatus;

        if (payment.PaymentStatus.Equals("Paid", StringComparison.OrdinalIgnoreCase))
        {
            TempData["SuccessMessage"] = message;
        }
        else
        {
            TempData["ErrorMessage"] = message;
        }
    }

    private string? Token() => HttpContext.Session.GetString("AccessToken") ?? Request.Cookies["AccessToken"];

    private IActionResult RedirectToLogin(int orderId) =>
        RedirectToAction("Login", "Auth", new { returnUrl = $"/Payment/PayPal?orderId={orderId}" });
}
