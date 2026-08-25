using CloneEbay_FE.Models;
using CloneEbay_FE.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CloneEbay_FE.Controllers
{
    public class ReturnRequestController : Controller
    {
        private readonly IApiClientService _apiClient;
        private readonly IWebHostEnvironment _env;

        public ReturnRequestController(IApiClientService apiClient, IWebHostEnvironment env)
        {
            _apiClient = apiClient;
            _env = env;
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

            var role = HttpContext.Session.GetString("Role") ?? string.Empty;
            var isSeller = role.Equals("Seller", StringComparison.OrdinalIgnoreCase);

            var endpoint = isSeller ? "return-requests/seller" : "return-requests";
            var response = await _apiClient.GetAsync<List<ReturnRequestViewModel>>(endpoint, token);
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

        // GET: /ReturnRequest/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var token = GetAccessToken();
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToLogin();
            }

            var response = await _apiClient.GetAsync<ReturnRequestViewModel>($"return-requests/{id}", token);
            if (response?.Success == true && response.Data != null)
            {
                return View(response.Data);
            }

            TempData["ErrorMessage"] = response?.Message ?? "Không tìm thấy yêu cầu hoàn trả.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /ReturnRequest/Create
        [HttpGet]
        public async Task<IActionResult> Create(int? orderId, int? orderItemId)
        {
            var token = GetAccessToken();
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToLogin();
            }

            var ordersRes = await _apiClient.GetAsync<List<OrderViewModel>>("orders/my-orders", token);
            var deliveredOrders = ordersRes?.Data?.Where(o =>
                string.Equals(o.Status, "Delivered", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(o.Status, "Return Requested", StringComparison.OrdinalIgnoreCase)).ToList() ?? new List<OrderViewModel>();

            ViewBag.DeliveredOrders = deliveredOrders;

            return View(new CreateReturnRequestViewModel
            {
                OrderId = orderId ?? 0,
                OrderItemId = orderItemId
            });
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
                var ordersRes = await _apiClient.GetAsync<List<OrderViewModel>>("orders/my-orders", token);
                ViewBag.DeliveredOrders = ordersRes?.Data?.Where(o =>
                    string.Equals(o.Status, "Delivered", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(o.Status, "Return Requested", StringComparison.OrdinalIgnoreCase)).ToList() ?? new List<OrderViewModel>();
                return View(model);
            }

            var uploadedImageUrls = new List<string>();
            if (model.EvidenceFiles != null && model.EvidenceFiles.Any())
            {
                var uploadFolder = Path.Combine(_env.WebRootPath, "uploads", "returns");
                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                foreach (var file in model.EvidenceFiles)
                {
                    if (file.Length > 0)
                    {
                        var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                        var filePath = Path.Combine(uploadFolder, fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                        uploadedImageUrls.Add($"/uploads/returns/{fileName}");
                    }
                }
            }

            var fullReason = $"[{model.SelectedReasonCategory}] {model.DetailedReason.Trim()}";

            var payload = new
            {
                orderId = model.OrderId,
                returnEntireOrder = model.ReturnEntireOrder,
                selectedOrderItemIds = model.SelectedOrderItemIds,
                orderItemId = model.OrderItemId,
                productId = model.ProductId,
                reason = fullReason,
                evidences = uploadedImageUrls
            };

            var response = await _apiClient.PostAsync<List<ReturnRequestViewModel>>("return-requests", payload, token);
            if (response?.Success == true)
            {
                TempData["SuccessMessage"] = response.Message ?? "Gửi yêu cầu hoàn trả thành công.";
                if (response.Data != null && response.Data.Count == 1)
                {
                    return RedirectToAction(nameof(Details), new { id = response.Data.First().Id });
                }
                return RedirectToAction(nameof(Index));
            }

            if (response?.StatusCode == StatusCodes.Status401Unauthorized)
            {
                return RedirectToLogin();
            }

            ModelState.AddModelError(string.Empty, response?.Message ?? "Không thể gửi yêu cầu hoàn trả.");

            var reLoadOrders = await _apiClient.GetAsync<List<OrderViewModel>>("orders/my-orders", token);
            ViewBag.DeliveredOrders = reLoadOrders?.Data?.Where(o =>
                string.Equals(o.Status, "Delivered", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(o.Status, "Return Requested", StringComparison.OrdinalIgnoreCase)).ToList() ?? new List<OrderViewModel>();

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

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: /ReturnRequest/UpdateTracking/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateTracking(int id, string trackingNumber)
        {
            var token = GetAccessToken();
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToLogin();
            }

            var response = await _apiClient.PutAsync<ReturnRequestViewModel>($"return-requests/{id}/tracking", new { trackingNumber }, token);
            if (response?.Success == true)
            {
                TempData["SuccessMessage"] = response.Message ?? "Đã cập nhật mã vận chuyển gửi trả hàng.";
            }
            else
            {
                TempData["ErrorMessage"] = response?.Message ?? "Không thể cập nhật mã vận chuyển.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: /ReturnRequest/ConfirmReturned/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmReturned(int id)
        {
            var token = GetAccessToken();
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToLogin();
            }

            var response = await _apiClient.PutAsync<ReturnRequestViewModel>($"return-requests/{id}/confirm-returned", null, token);
            if (response?.Success == true)
            {
                TempData["SuccessMessage"] = response.Message ?? "Xác nhận đã nhận sản phẩm và hoàn tiền thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = response?.Message ?? "Không thể xác nhận nhận hàng.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: /ReturnRequest/Escalate/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Escalate(int id, string reason)
        {
            var token = GetAccessToken();
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToLogin();
            }

            var response = await _apiClient.PutAsync<ReturnRequestViewModel>($"return-requests/{id}/escalate", new { reason }, token);
            if (response?.Success == true)
            {
                TempData["SuccessMessage"] = response.Message ?? "Đã gửi yêu cầu Admin can thiệp (Ask eBay to step in).";
            }
            else
            {
                TempData["ErrorMessage"] = response?.Message ?? "Không thể gửi yêu cầu can thiệp.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: /ReturnRequest/UpdateStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status, string? refundType, decimal? refundAmount, string? adminNotes)
        {
            var token = GetAccessToken();
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToLogin();
            }

            var payload = new
            {
                status,
                refundType,
                refundAmount,
                adminNotes
            };

            var response = await _apiClient.PutAsync<ReturnRequestViewModel>($"return-requests/{id}/status", payload, token);
            if (response?.Success == true)
            {
                TempData["SuccessMessage"] = response.Message ?? "Đã cập nhật trạng thái yêu cầu hoàn trả.";
            }
            else
            {
                TempData["ErrorMessage"] = response?.Message ?? "Không thể cập nhật trạng thái.";
            }

            return RedirectToAction(nameof(Details), new { id });
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
