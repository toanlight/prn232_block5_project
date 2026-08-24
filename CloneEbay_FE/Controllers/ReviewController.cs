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

        // GET: /Review?productId=1
        [HttpGet]
        public async Task<IActionResult> Index(int productId = 1)
        {
            var response = await _apiClient.GetAsync<ProductReviewSummaryViewModel>($"products/{productId}/reviews");
            if (response?.Success == true && response.Data != null)
            {
                return View(response.Data);
            }

            TempData["ErrorMessage"] = response?.Message ?? "Không thể tải danh sách đánh giá sản phẩm.";
            return View(new ProductReviewSummaryViewModel { ProductId = productId });
        }

        // GET: /Review/MyReviews
        [HttpGet]
        public async Task<IActionResult> MyReviews()
        {
            var token = GetAccessToken();
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToLogin();
            }

            var response = await _apiClient.GetAsync<List<ReviewViewModel>>("reviews/my-reviews", token);
            if (response?.Success == true)
            {
                return View(response.Data ?? new List<ReviewViewModel>());
            }

            TempData["ErrorMessage"] = response?.Message ?? "Không thể tải danh sách đánh giá của bạn.";
            return View(new List<ReviewViewModel>());
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
                return RedirectToAction(nameof(Index), new { productId = model.ProductId });
            }

            if (response?.StatusCode == StatusCodes.Status401Unauthorized)
            {
                return RedirectToLogin();
            }

            ModelState.AddModelError(string.Empty, response?.Message ?? "Không thể gửi đánh giá.");
            return View(model);
        }

        // POST: /Review/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int productId)
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

            return RedirectToAction(nameof(Index), new { productId });
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
