using CloneEbay_FE.Models;
using CloneEbay_FE.Services;
using Microsoft.AspNetCore.Mvc;

namespace CloneEbay_FE.Controllers
{
    public class AuthController : Controller
    {
        private readonly IApiClientService _apiClient;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IApiClientService apiClient, ILogger<AuthController> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        // GET: /Auth/Register
        [HttpGet]
        public IActionResult Register()
        {
            if (HttpContext.Session.GetString("AccessToken") != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View(new RegisterViewModel());
        }

        // POST: /Auth/Register (SCRUM-6 Sub-task 1.1, 1.2, 1.3)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var response = await _apiClient.PostAsync<UserInfoModel>("auth/register", model);

            if (response != null && response.Success)
            {
                TempData["SuccessMessage"] = response.Message ?? "Đăng ký thành công! Vui lòng nhập mã OTP đã gửi đến email.";
                return RedirectToAction("VerifyEmail", new { email = model.Email });
            }

            ModelState.AddModelError(string.Empty, response?.Message ?? "Đăng ký thất bại. Vui lòng thử lại.");
            return View(model);
        }

        // GET: /Auth/VerifyEmail (SCRUM-6 Sub-task 1.3, SCRUM-23)
        [HttpGet]
        public IActionResult VerifyEmail(string? email)
        {
            var model = new VerifyOtpViewModel
            {
                Email = email ?? string.Empty
            };
            return View(model);
        }

        // POST: /Auth/VerifyEmail
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyEmail(VerifyOtpViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var response = await _apiClient.PostAsync<bool>("auth/verify-otp", model);

            if (response != null && response.Success)
            {
                TempData["SuccessMessage"] = "Kích hoạt tài khoản thành công! Bạn có thể đăng nhập ngay bây giờ.";
                return RedirectToAction("Login");
            }

            ModelState.AddModelError(string.Empty, response?.Message ?? "Mã xác thực không hợp lệ.");
            return View(model);
        }

        // POST: /Auth/ResendOtp
        [HttpPost]
        public async Task<IActionResult> ResendOtp(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return Json(new { success = false, message = "Email không hợp lệ." });
            }

            var response = await _apiClient.PostAsync<bool>("auth/resend-otp", new { Email = email });
            return Json(new { success = response?.Success ?? false, message = response?.Message ?? "Không thể gửi lại mã OTP." });
        }

        // GET: /Auth/Login (SCRUM-7 Sub-task 2.2)
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (HttpContext.Session.GetString("AccessToken") != null)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel());
        }

        // POST: /Auth/Login (SCRUM-7 Sub-task 2.1 & 2.2)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var response = await _apiClient.PostAsync<AuthResponseModel>("auth/login", model);

            if (response != null && response.Success && response.Data != null)
            {
                // Lưu session & Cookie token
                HttpContext.Session.SetString("AccessToken", response.Data.AccessToken);
                HttpContext.Session.SetString("RefreshToken", response.Data.RefreshToken);
                HttpContext.Session.SetString("Username", response.Data.User.Username);
                HttpContext.Session.SetString("FullName", response.Data.User.FullName ?? response.Data.User.Username);
                HttpContext.Session.SetString("Role", response.Data.User.Role ?? "User");
                HttpContext.Session.SetString("UserRole", response.Data.User.Role ?? "User");
                HttpContext.Session.SetInt32("UserId", response.Data.User.Id);
                HttpContext.Session.SetString("AvatarUrl", response.Data.User.AvatarUrl ?? "");

                // Lưu cookie nếu RememberMe
                if (model.RememberMe)
                {
                    Response.Cookies.Append("AccessToken", response.Data.AccessToken, new CookieOptions
                    {
                        HttpOnly = true,
                        Expires = DateTimeOffset.UtcNow.AddDays(7),
                        SameSite = SameSiteMode.Lax
                    });
                }

                TempData["SuccessMessage"] = $"Chào mừng bạn trở lại, {response.Data.User.FullName ?? response.Data.User.Username}!";

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "Home");
            }

            if (response?.Message != null && response.Message.Contains("chưa được kích hoạt"))
            {
                TempData["WarningMessage"] = response.Message;
                return RedirectToAction("VerifyEmail", new { email = model.UsernameOrEmail });
            }

            ModelState.AddModelError(string.Empty, response?.Message ?? "Đăng nhập thất bại. Vui lòng kiểm tra lại thông tin.");
            return View(model);
        }

        // GET: /Auth/Logout (SCRUM-7 Sub-task 2.1)
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            var token = HttpContext.Session.GetString("AccessToken") ?? Request.Cookies["AccessToken"];
            if (!string.IsNullOrEmpty(token))
            {
                await _apiClient.PostAsync<bool>("auth/logout", null, token);
            }

            HttpContext.Session.Clear();
            Response.Cookies.Delete("AccessToken");

            TempData["SuccessMessage"] = "Bạn đã đăng xuất thành công.";
            return RedirectToAction("Login");
        }
    }
}
