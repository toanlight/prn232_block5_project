using CloneEbay_FE.Models;
using CloneEbay_FE.Services;
using Microsoft.AspNetCore.Mvc;

namespace CloneEbay_FE.Controllers;

public sealed class OrdersController(IApiClientService apiClient) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string? status = null, int page = 1)
    {
        var token = Token();
        if (token is null) return RedirectToLogin("/Orders");

        page = Math.Max(1, page);
        var endpoint = $"orders?page={page}&pageSize=10";
        if (!string.IsNullOrWhiteSpace(status) && !status.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            endpoint += $"&status={Uri.EscapeDataString(status)}";
        }

        var response = await apiClient.GetAsync<OrderHistoryPageViewModel>(endpoint, token);
        if (response?.StatusCode == StatusCodes.Status401Unauthorized)
        {
            return RedirectToLogin("/Orders");
        }

        if (response?.Success != true || response.Data is null)
        {
            TempData["ErrorMessage"] = response?.Message ?? "Không thể tải lịch sử đơn hàng.";
            return View(new OrderHistoryPageViewModel
            {
                Page = page,
                PageSize = 10,
                StatusFilter = status ?? "all"
            });
        }

        return View(response.Data);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var token = Token();
        if (token is null) return RedirectToLogin($"/Orders/Details/{id}");

        var response = await apiClient.GetAsync<OrderDetailViewModel>($"orders/{id}", token);
        if (response?.StatusCode == StatusCodes.Status401Unauthorized)
        {
            return RedirectToLogin($"/Orders/Details/{id}");
        }

        if (response?.Success != true || response.Data is null)
        {
            TempData["ErrorMessage"] = response?.Message ?? "Không thể tải chi tiết đơn hàng.";
            return RedirectToAction(nameof(Index));
        }

        return View(response.Data);
    }

    private string? Token() => HttpContext.Session.GetString("AccessToken") ?? Request.Cookies["AccessToken"];

    private IActionResult RedirectToLogin(string returnUrl) =>
        RedirectToAction("Login", "Auth", new { returnUrl });
}
