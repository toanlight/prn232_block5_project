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
}
