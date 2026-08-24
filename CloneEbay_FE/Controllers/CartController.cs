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

        var cartResponse = await apiClient.GetAsync<CartViewModel>("cart", Token());
        var cart = cartResponse?.Data ?? new CartViewModel();

        // Lấy danh sách coupons active
        var couponResponse = await apiClient.GetAsync<List<CouponViewModel>>("coupons");
        if (couponResponse?.Success == true && couponResponse.Data != null)
        {
            cart.AvailableCoupons = couponResponse.Data;
        }

        // Đọc thông tin coupon áp dụng cho từng item trong giỏ hàng
        foreach (var item in cart.Items)
        {
            var appliedCode = HttpContext.Session.GetString($"AppliedCouponCode_{item.ProductId}");
            var discountStr = HttpContext.Session.GetString($"DiscountAmount_{item.ProductId}");

            if (!string.IsNullOrEmpty(appliedCode) && decimal.TryParse(discountStr, out var discount))
            {
                item.AppliedCouponCode = appliedCode;
                item.DiscountAmount = discount;
            }
        }

        return View(cart);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ApplyCoupon(int productId, string code)
    {
        if (Token() is null) return RedirectToAction("Login", "Auth");
        if (string.IsNullOrWhiteSpace(code))
        {
            TempData["ErrorMessage"] = "Vui lòng nhập mã giảm giá.";
            return RedirectToAction(nameof(Index));
        }

        var cartResponse = await apiClient.GetAsync<CartViewModel>("cart", Token());
        var cart = cartResponse?.Data ?? new CartViewModel();
        var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);

        if (item == null)
        {
            TempData["ErrorMessage"] = "Sản phẩm không có trong giỏ hàng.";
            return RedirectToAction(nameof(Index));
        }

        var applyReq = new
        {
            code = code.Trim(),
            productId = productId,
            originalPrice = item.LineTotal
        };

        var response = await apiClient.PostAsync<ApplyCouponResultViewModel>("coupons/apply", applyReq, Token());
        if (response?.Success == true && response.Data != null)
        {
            HttpContext.Session.SetString($"AppliedCouponCode_{productId}", response.Data.Code);
            HttpContext.Session.SetString($"DiscountAmount_{productId}", response.Data.DiscountAmount.ToString("F2"));

            var appliedCoupons = GetAppliedCouponsFromSession();
            appliedCoupons[productId] = response.Data.Code;
            HttpContext.Session.SetString("AppliedCoupons", System.Text.Json.JsonSerializer.Serialize(appliedCoupons));

            TempData["SuccessMessage"] = response.Message ?? $"Áp dụng mã {response.Data.Code} cho sản phẩm thành công! Giảm {response.Data.DiscountAmount:N0} ₫";
        }
        else
        {
            TempData["ErrorMessage"] = response?.Message ?? "Mã giảm giá không hợp lệ hoặc không áp dụng cho sản phẩm này.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult RemoveCoupon(int productId)
    {
        HttpContext.Session.Remove($"AppliedCouponCode_{productId}");
        HttpContext.Session.Remove($"DiscountAmount_{productId}");

        var appliedCoupons = GetAppliedCouponsFromSession();
        if (appliedCoupons.ContainsKey(productId))
        {
            appliedCoupons.Remove(productId);
            HttpContext.Session.SetString("AppliedCoupons", System.Text.Json.JsonSerializer.Serialize(appliedCoupons));
        }

        TempData["SuccessMessage"] = "Đã bỏ áp dụng mã giảm giá cho sản phẩm.";
        return RedirectToAction(nameof(Index));
    }

    private Dictionary<int, string> GetAppliedCouponsFromSession()
    {
        var sessionData = HttpContext.Session.GetString("AppliedCoupons");
        if (string.IsNullOrEmpty(sessionData)) return new Dictionary<int, string>();
        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<Dictionary<int, string>>(sessionData) ?? new Dictionary<int, string>();
        }
        catch
        {
            return new Dictionary<int, string>();
        }
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
    public async Task<IActionResult> Update(int productId, int quantity)
    {
        await apiClient.PutAsync<CartViewModel>($"cart/items/{productId}", new { quantity }, Token());
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int productId)
    {
        await apiClient.DeleteAsync<CartViewModel>($"cart/items/{productId}", Token());
        // Cũng xoá luôn coupon đã gắn với sản phẩm đó nếu xoá khỏi giỏ
        HttpContext.Session.Remove($"AppliedCouponCode_{productId}");
        HttpContext.Session.Remove($"DiscountAmount_{productId}");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Sync([FromBody] List<AddCartItemViewModel> items)
    {
        if (Token() is null) return Unauthorized();
        var response = await apiClient.PostAsync<CartViewModel>("cart/merge", items.Select(x => new { x.ProductId, x.Quantity }), Token());
        return Json(new { success = response?.Success == true, message = response?.Message });
    }
}
