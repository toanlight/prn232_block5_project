using CloneEbay_FE.Models;
using CloneEbay_FE.Services;
using Microsoft.AspNetCore.Mvc;

namespace CloneEbay_FE.Controllers
{
    public class ProfileController : Controller
    {
        private readonly IApiClientService _apiClient;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(IApiClientService apiClient, ILogger<ProfileController> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        private string? GetAccessToken()
        {
            return HttpContext.Session.GetString("AccessToken") ?? Request.Cookies["AccessToken"];
        }

        // GET: /Profile (SCRUM-7 Sub-task 2.2 & 2.3)
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var token = GetAccessToken();
            if (string.IsNullOrEmpty(token))
            {
                TempData["WarningMessage"] = "Vui lòng đăng nhập để xem thông tin cá nhân.";
                return RedirectToAction("Login", "Auth", new { returnUrl = "/Profile" });
            }

            var response = await _apiClient.GetAsync<ProfileViewModel>("user/profile", token);
            if (response != null && response.Success && response.Data != null)
            {
                return View(response.Data);
            }

            TempData["ErrorMessage"] = response?.Message ?? "Không thể tải thông tin cá nhân.";
            return RedirectToAction("Login", "Auth");
        }

        // POST: /Profile/Update (SCRUM-7 Sub-task 2.3)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(ProfileViewModel model)
        {
            var token = GetAccessToken();
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Auth");
            }

            var updateDto = new
            {
                FullName = model.FullName,
                Phone = model.Phone,
                AvatarUrl = model.AvatarUrl
            };

            var response = await _apiClient.PutAsync<ProfileViewModel>("user/profile", updateDto, token);

            if (response != null && response.Success)
            {
                HttpContext.Session.SetString("FullName", model.FullName);
                TempData["SuccessMessage"] = "Cập nhật thông tin cá nhân thành công!";
                return RedirectToAction("Index");
            }

            TempData["ErrorMessage"] = response?.Message ?? "Cập nhật thất bại.";
            return View("Index", model);
        }

        // POST: /Profile/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var token = GetAccessToken();
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Dữ liệu đổi mật khẩu không hợp lệ.";
                return RedirectToAction("Index");
            }

            var response = await _apiClient.PostAsync<bool>("user/change-password", model, token);

            if (response != null && response.Success)
            {
                TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = response?.Message ?? "Đổi mật khẩu thất bại. Vui lòng kiểm tra lại mật khẩu cũ.";
            }

            return RedirectToAction("Index");
        }
    }
}
