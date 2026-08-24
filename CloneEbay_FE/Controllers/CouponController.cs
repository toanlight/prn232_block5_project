using CloneEbay_FE.Models;
using CloneEbay_FE.Services;
using Microsoft.AspNetCore.Mvc;

namespace CloneEbay_FE.Controllers
{
    public class CouponController : Controller
    {
        private readonly IApiClientService _apiClient;

        public CouponController(IApiClientService apiClient)
        {
            _apiClient = apiClient;
        }

        // GET: /Coupon
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var response = await _apiClient.GetAsync<List<CouponViewModel>>("coupons");
            var coupons = response?.Success == true ? (response.Data ?? new List<CouponViewModel>()) : new List<CouponViewModel>();
            
            ViewBag.Coupons = coupons;
            return View(new ApplyCouponViewModel { ProductId = 1, OriginalPrice = 100000 });
        }

        // POST: /Coupon/Apply
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(ApplyCouponViewModel model)
        {
            var responseList = await _apiClient.GetAsync<List<CouponViewModel>>("coupons");
            ViewBag.Coupons = responseList?.Success == true ? (responseList.Data ?? new List<CouponViewModel>()) : new List<CouponViewModel>();

            var token = GetAccessToken();
            if (string.IsNullOrEmpty(token))
            {
                TempData["WarningMessage"] = "Vui lòng đăng nhập để áp dụng mã giảm giá.";
                return View("Index", model);
            }

            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            var response = await _apiClient.PostAsync<ApplyCouponResultViewModel>("coupons/apply", model, token);
            if (response?.Success == true && response.Data != null)
            {
                ViewBag.Result = response.Data;
                TempData["SuccessMessage"] = response.Message ?? "Áp dụng mã giảm giá thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = response?.Message ?? "Không thể áp dụng mã giảm giá.";
            }

            return View("Index", model);
        }

        // GET: /Coupon/Create
        [HttpGet]
        public IActionResult Create()
        {
            if (string.IsNullOrEmpty(GetAccessToken()))
            {
                return RedirectToLogin();
            }

            return View(new CreateCouponViewModel { ProductId = 1, DiscountPercent = 10, MaxUsage = 20 });
        }

        // POST: /Coupon/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCouponViewModel model)
        {
            var token = GetAccessToken();
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToLogin();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var response = await _apiClient.PostAsync<CouponViewModel>("coupons", model, token);
            if (response?.Success == true)
            {
                TempData["SuccessMessage"] = response.Message ?? "Tạo mã giảm giá mới thành công!";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, response?.Message ?? "Không thể tạo mã giảm giá.");
            return View(model);
        }

        // POST: /Coupon/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var token = GetAccessToken();
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToLogin();
            }

            var response = await _apiClient.DeleteAsync<bool>($"coupons/{id}", token);
            if (response?.Success == true)
            {
                TempData["SuccessMessage"] = response.Message ?? "Xóa mã giảm giá thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = response?.Message ?? "Không thể xóa mã giảm giá.";
            }

            return RedirectToAction(nameof(Index));
        }

        private string? GetAccessToken()
        {
            return HttpContext.Session.GetString("AccessToken") ?? Request.Cookies["AccessToken"];
        }

        private IActionResult RedirectToLogin()
        {
            TempData["WarningMessage"] = "Vui lòng đăng nhập để quản lý mã giảm giá.";
            return RedirectToAction("Login", "Auth", new { returnUrl = "/Coupon" });
        }
    }
}
