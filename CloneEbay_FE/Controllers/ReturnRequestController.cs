using CloneEbay_FE.Models;
using CloneEbay_FE.Services;
using Microsoft.AspNetCore.Mvc;

namespace CloneEbay_FE.Controllers
{
    public class ReturnRequestController : Controller
    {
        private readonly IApiClientService _apiClient;

        public ReturnRequestController(IApiClientService apiClient)
        {
            _apiClient = apiClient;
        }

        // GET: /ReturnRequest
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var token = GetAccessToken();
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToLogin();
            }

            var response = await _apiClient.GetAsync<List<ReturnRequestViewModel>>("return-requests", token);
            if (response?.Success == true)
            {
                return View(response.Data ?? new List<ReturnRequestViewModel>());
            }

            if (response?.StatusCode == StatusCodes.Status401Unauthorized)
            {
                return RedirectToLogin();
            }

            TempData["ErrorMessage"] = response?.Message ?? "Không thể tải danh sách yêu cầu hoàn trả.";
            return View(new List<ReturnRequestViewModel>());
        }

        // GET: /ReturnRequest/Create
        [HttpGet]
        public IActionResult Create(int? orderId)
        {
            if (string.IsNullOrEmpty(GetAccessToken()))
            {
                return RedirectToLogin();
            }

            return View(new CreateReturnRequestViewModel { OrderId = orderId ?? 0 });
        }

        // POST: /ReturnRequest/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateReturnRequestViewModel model)
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

            var response = await _apiClient.PostAsync<ReturnRequestViewModel>("return-requests", model, token);
            if (response?.Success == true)
            {
                TempData["SuccessMessage"] = response.Message ?? "Gửi yêu cầu hoàn trả thành công.";
                return RedirectToAction(nameof(Index));
            }

            if (response?.StatusCode == StatusCodes.Status401Unauthorized)
            {
                return RedirectToLogin();
            }

            ModelState.AddModelError(string.Empty, response?.Message ?? "Không thể gửi yêu cầu hoàn trả.");
            return View(model);
        }

        // POST: /ReturnRequest/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var token = GetAccessToken();
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToLogin();
            }

            var response = await _apiClient.PutAsync<ReturnRequestViewModel>($"return-requests/{id}/cancel", null, token);
            if (response?.Success == true)
            {
                TempData["SuccessMessage"] = response.Message ?? "Đã huỷ yêu cầu hoàn trả.";
            }
            else
            {
                TempData["ErrorMessage"] = response?.Message ?? "Không thể huỷ yêu cầu hoàn trả.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /ReturnRequest/UpdateStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var token = GetAccessToken();
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToLogin();
            }

            var response = await _apiClient.PutAsync<ReturnRequestViewModel>($"return-requests/{id}/status", new { status }, token);
            if (response?.Success == true)
            {
                TempData["SuccessMessage"] = response.Message ?? "Đã cập nhật trạng thái yêu cầu.";
            }
            else
            {
                TempData["ErrorMessage"] = response?.Message ?? "Không thể cập nhật trạng thái.";
            }

            return RedirectToAction(nameof(Index));
        }

        private string? GetAccessToken()
        {
            return HttpContext.Session.GetString("AccessToken") ?? Request.Cookies["AccessToken"];
        }

        private IActionResult RedirectToLogin()
        {
            TempData["WarningMessage"] = "Vui lòng đăng nhập để quản lý yêu cầu hoàn trả.";
            return RedirectToAction("Login", "Auth", new { returnUrl = "/ReturnRequest" });
        }
    }
}
