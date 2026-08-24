using CloneEbay_FE.Models;
using CloneEbay_FE.Services;
using Microsoft.AspNetCore.Mvc;

namespace CloneEbay_FE.Controllers;

public sealed class OrdersController(IApiClientService apiClient) : Controller
{
    private string? Token() => HttpContext.Session.GetString("AccessToken") ?? Request.Cookies["AccessToken"];

    public async Task<IActionResult> Index()
    {
        if (Token() is null)
        {
            return RedirectToAction("Login", "Auth", new { returnUrl = "/Orders" });
        }

        var response = await apiClient.GetAsync<List<OrderHistoryItemViewModel>>("orders/my-orders", Token());
        var orders = response?.Success == true ? (response.Data ?? []) : [];
        return View(orders);
    }

    public async Task<IActionResult> Details(int id)
    {
        if (Token() is null)
        {
            return RedirectToAction("Login", "Auth", new { returnUrl = $"/Orders/Details/{id}" });
        }

        var response = await apiClient.GetAsync<OrderHistoryItemViewModel>($"orders/{id}", Token());
        if (response?.Success != true || response.Data is null)
        {
            TempData["ErrorMessage"] = response?.Message ?? "Không tìm thấy thông tin đơn hàng.";
            return RedirectToAction(nameof(Index));
        }

        return View(response.Data);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmReceived(int id)
    {
        if (Token() is null)
        {
            return RedirectToAction("Login", "Auth");
        }

        var response = await apiClient.PostAsync<OrderHistoryItemViewModel>($"orders/{id}/confirm-received", null, Token());
        if (response?.Success == true)
        {
            TempData["SuccessMessage"] = "Đã xác nhận đã nhận hàng thành công!";
        }
        else
        {
            TempData["ErrorMessage"] = response?.Message ?? "Không thể xác nhận nhận hàng.";
        }

        return RedirectToAction(nameof(Details), new { id });
    }
}
