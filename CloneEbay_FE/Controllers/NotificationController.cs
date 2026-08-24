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
        public IActionResult Index()
        {
            var token = GetAccessToken();
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToLogin();
            }

            return View();
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
