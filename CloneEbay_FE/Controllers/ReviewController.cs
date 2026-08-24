using CloneEbay_FE.Models;
using CloneEbay_FE.Services;
using Microsoft.AspNetCore.Mvc;

namespace CloneEbay_FE.Controllers
{
    public class ReviewController : Controller
    {
        private readonly IApiClientService _apiClient;

        public ReviewController(IApiClientService apiClient)
        {
            _apiClient = apiClient;
        }

        // GET: /Review
        [HttpGet]
        public IActionResult Index()
        {
            return RedirectToAction("Orders", "Profile");
        }

        // GET: /Review/MyReviews -> Redirect directly to My Orders
        [HttpGet]
        public IActionResult MyReviews()
        {
            return RedirectToAction("Orders", "Profile");
        }

        // GET: /Review/Create?productId=1
        [HttpGet]
        public async Task<IActionResult> Create(int productId = 1)
        {
            if (string.IsNullOrEmpty(GetAccessToken()))
            {
                return RedirectToLogin();
            }

            var productRes = await _apiClient.GetAsync<ProductDetailViewModel>($"products/{productId}");
            if (productRes?.Success == true && productRes.Data != null)
            {
                ViewBag.ProductTitle = productRes.Data.Title;
                ViewBag.ProductImage = productRes.Data.ImageUrl;
            }

            return View(new CreateReviewViewModel { ProductId = productId, Rating = 5 });
        }

        // POST: /Review/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateReviewViewModel model)
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

            var response = await _apiClient.PostAsync<ReviewViewModel>("reviews", model, token);
            if (response?.Success == true)
            {
                TempData["SuccessMessage"] = response.Message ?? "Gửi đánh giá sản phẩm thành công!";
                return RedirectToAction("Orders", "Profile");
            }

            if (response?.StatusCode == StatusCodes.Status401Unauthorized)
            {
                return RedirectToLogin();
            }

            ModelState.AddModelError(string.Empty, response?.Message ?? "Không thể gửi đánh giá.");
            return View(model);
        }

        // POST: /Review/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int reviewId, int rating, string? comment, string? returnUrl = null)
        {
            var token = GetAccessToken();
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToLogin();
            }

            var editDto = new { rating = rating, comment = comment };
            var response = await _apiClient.PutAsync<ReviewViewModel>($"reviews/{reviewId}", editDto, token);
            if (response?.Success == true)
            {
                TempData["SuccessMessage"] = response.Message ?? "Cập nhật đánh giá thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = response?.Message ?? "Không thể cập nhật đánh giá.";
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Orders", "Profile");
        }

        // POST: /Review/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int? productId = null, string? returnUrl = null)
        {
            var token = GetAccessToken();
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToLogin();
            }

            var response = await _apiClient.DeleteAsync<bool>($"reviews/{id}", token);
            if (response?.Success == true)
            {
                TempData["SuccessMessage"] = response.Message ?? "Xóa đánh giá thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = response?.Message ?? "Không thể xóa đánh giá.";
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            if (productId.HasValue && productId.Value > 0)
            {
                return RedirectToAction(nameof(Index), new { productId = productId.Value });
            }

            return RedirectToAction("Orders", "Profile");
        }

        private string? GetAccessToken()
        {
            return HttpContext.Session.GetString("AccessToken") ?? Request.Cookies["AccessToken"];
        }

        private IActionResult RedirectToLogin()
        {
            TempData["WarningMessage"] = "Vui lòng đăng nhập để thực hiện đánh giá sản phẩm.";
            return RedirectToAction("Login", "Auth", new { returnUrl = "/Review" });
        }
    }
}
