using CloneEbay_FE.Models;
using CloneEbay_FE.Services;
using Microsoft.AspNetCore.Mvc;

namespace CloneEbay_FE.Controllers
{
    public class AddressController : Controller
    {
        private readonly IApiClientService _apiClient;

        public AddressController(IApiClientService apiClient)
        {
            _apiClient = apiClient;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var token = GetAccessToken();
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToLogin();
            }

            var response = await _apiClient.GetAsync<List<AddressViewModel>>("addresses", token);
            if (response?.Success == true)
            {
                return View(response.Data ?? new List<AddressViewModel>());
            }

            if (response?.StatusCode == StatusCodes.Status401Unauthorized)
            {
                HttpContext.Session.Clear();
                Response.Cookies.Delete("AccessToken");
                return RedirectToLogin();
            }

            TempData["ErrorMessage"] = response?.Message ?? "Không thể tải danh sách địa chỉ giao hàng.";
            return View(new List<AddressViewModel>());
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (string.IsNullOrEmpty(GetAccessToken()))
            {
                return RedirectToLogin();
            }

            return View(new AddressViewModel { Country = "Việt Nam" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AddressViewModel model)
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

            var response = await _apiClient.PostAsync<AddressViewModel>("addresses", model, token);
            if (response?.Success == true)
            {
                TempData["SuccessMessage"] = response.Message;
                return RedirectToAction(nameof(Index));
            }

            if (response?.StatusCode == StatusCodes.Status401Unauthorized)
            {
                return RedirectToLogin();
            }

            AddApiError(response, "Không thể thêm địa chỉ giao hàng.");
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var token = GetAccessToken();
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToLogin();
            }

            var response = await _apiClient.GetAsync<AddressViewModel>($"addresses/{id}", token);
            if (response?.Success == true && response.Data != null)
            {
                return View(response.Data);
            }

            return HandleApiFailure(response, "Không tìm thấy địa chỉ giao hàng.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AddressViewModel model)
        {
            var token = GetAccessToken();
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToLogin();
            }

            model.Id = id;
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var response = await _apiClient.PutAsync<AddressViewModel>($"addresses/{id}", model, token);
            if (response?.Success == true)
            {
                TempData["SuccessMessage"] = response.Message;
                return RedirectToAction(nameof(Index));
            }

            if (response?.StatusCode == StatusCodes.Status401Unauthorized)
            {
                return RedirectToLogin();
            }

            AddApiError(response, "Không thể cập nhật địa chỉ giao hàng.");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var token = GetAccessToken();
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToLogin();
            }

            var response = await _apiClient.DeleteAsync<bool>($"addresses/{id}", token);
            SetOperationMessage(response, "Xóa địa chỉ giao hàng thành công.", "Không thể xóa địa chỉ giao hàng.");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetDefault(int id)
        {
            var token = GetAccessToken();
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToLogin();
            }

            var response = await _apiClient.PostAsync<AddressViewModel>($"addresses/{id}/set-default", null, token);
            SetOperationMessage(response, "Đã chọn địa chỉ mặc định.", "Không thể chọn địa chỉ mặc định.");
            return RedirectToAction(nameof(Index));
        }

        private string? GetAccessToken()
        {
            return HttpContext.Session.GetString("AccessToken") ?? Request.Cookies["AccessToken"];
        }

        private IActionResult RedirectToLogin()
        {
            TempData["WarningMessage"] = "Vui lòng đăng nhập để quản lý địa chỉ giao hàng.";
            return RedirectToAction("Login", "Auth", new { returnUrl = "/Address" });
        }

        private IActionResult HandleApiFailure<T>(ApiResponseModel<T>? response, string fallbackMessage)
        {
            if (response?.StatusCode == StatusCodes.Status401Unauthorized)
            {
                HttpContext.Session.Clear();
                Response.Cookies.Delete("AccessToken");
                return RedirectToLogin();
            }

            TempData["ErrorMessage"] = response?.Message ?? fallbackMessage;
            return RedirectToAction(nameof(Index));
        }

        private void AddApiError<T>(ApiResponseModel<T>? response, string fallbackMessage)
        {
            ModelState.AddModelError(string.Empty, response?.Message ?? fallbackMessage);
        }

        private void SetOperationMessage<T>(ApiResponseModel<T>? response, string successMessage, string errorMessage)
        {
            if (response?.Success == true)
            {
                TempData["SuccessMessage"] = response.Message ?? successMessage;
            }
            else
            {
                TempData["ErrorMessage"] = response?.Message ?? errorMessage;
            }
        }
    }
}
