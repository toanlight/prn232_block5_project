using CloneEbay_FE.Models;
using CloneEbay_FE.Services;
using Microsoft.AspNetCore.Mvc;

namespace CloneEbay_FE.Controllers
{
    public class NotificationController : Controller
    {
        private readonly IApiClientService _apiClient;

        public NotificationController(IApiClientService apiClient)
        {
            _apiClient = apiClient;
        }

        // GET: /Notification
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var token = GetAccessToken();
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToLogin();
            }

            var response = await _apiClient.GetAsync<List<NotificationViewModel>>("notification/my-notifications", token);
            var notifications = response?.Data ?? new List<NotificationViewModel>();

            return View(notifications);
        }

        // GET: /Notification/UnreadCount (AJAX)
        [HttpGet]
        public async Task<IActionResult> UnreadCount()
        {
            var token = GetAccessToken();
            if (string.IsNullOrEmpty(token))
            {
                return Json(new { unreadCount = 0 });
            }

            var response = await _apiClient.GetAsync<UnreadCountViewModel>("notification/unread-count", token);
            return Json(new { unreadCount = response?.Data?.UnreadCount ?? 0 });
        }

        // POST: /Notification/MarkAsRead (AJAX)
        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var token = GetAccessToken();
            if (string.IsNullOrEmpty(token))
            {
                return Json(new { success = false });
            }

            var response = await _apiClient.PutAsync<bool>($"notification/{id}/read", null, token);
            return Json(new { success = response?.Success == true });
        }

        // POST: /Notification/MarkAllAsRead (AJAX)
        [HttpPost]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var token = GetAccessToken();
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToLogin();
            }

            await _apiClient.PutAsync<bool>("notification/read-all", null, token);
            TempData["SuccessMessage"] = "Đã đánh dấu tất cả thông báo là đã đọc.";
            return RedirectToAction(nameof(Index));
        }

        private string? GetAccessToken()
        {
            return HttpContext.Session.GetString("AccessToken") ?? Request.Cookies["AccessToken"];
        }

        private IActionResult RedirectToLogin()
        {
            TempData["WarningMessage"] = "Vui lòng đăng nhập để xem danh sách thông báo.";
            return RedirectToAction("Login", "Auth", new { returnUrl = "/Notification" });
        }
    }
}
