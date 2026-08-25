using System.Security.Claims;
using ClonEbay_CoreAPI.Common.Models;
using ClonEbay_CoreAPI.DTOs.Notification;
using ClonEbay_CoreAPI.Exceptions;
using ClonEbay_CoreAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClonEbay_CoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        // ─── GET /api/Notification/my-notifications ───────────────────────────
        /// <summary>Lấy danh sách thông báo của người dùng hiện tại từ CSDL.</summary>
        [HttpGet("my-notifications")]
        [ProducesResponseType(typeof(ApiResponse<List<NotificationDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyNotifications()
        {
            return Ok(await _notificationService.GetUserNotificationsAsync(GetCurrentUserId(), GetCurrentUserRole()));
        }

        // ─── GET /api/Notification/unread-count ────────────────────────────────
        /// <summary>Lấy số lượng thông báo chưa đọc.</summary>
        [HttpGet("unread-count")]
        [ProducesResponseType(typeof(ApiResponse<UnreadCountDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUnreadCount()
        {
            return Ok(await _notificationService.GetUnreadCountAsync(GetCurrentUserId(), GetCurrentUserRole()));
        }

        // ─── PUT /api/Notification/{id}/read ──────────────────────────────────
        /// <summary>Đánh dấu một thông báo đã đọc.</summary>
        [HttpPut("{id:int}/read")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            return Ok(await _notificationService.MarkAsReadAsync(id, GetCurrentUserId()));
        }

        // ─── PUT /api/Notification/read-all ────────────────────────────────────
        /// <summary>Đánh dấu tất cả thông báo đã đọc.</summary>
        [HttpPut("read-all")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> MarkAllAsRead()
        {
            return Ok(await _notificationService.MarkAllAsReadAsync(GetCurrentUserId(), GetCurrentUserRole()));
        }

        // ─── Private helpers ──────────────────────────────────────────────────
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedException("Vui lòng đăng nhập để thực hiện chức năng này.");
            }
            return userId;
        }

        private string GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        }
    }
}
