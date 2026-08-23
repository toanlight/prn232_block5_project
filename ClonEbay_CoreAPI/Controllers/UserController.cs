using System.Security.Claims;
using ClonEbay_CoreAPI.Common.Models;
using ClonEbay_CoreAPI.DTOs.User;
using ClonEbay_CoreAPI.Exceptions;
using ClonEbay_CoreAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClonEbay_CoreAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedException("Vui lòng đăng nhập để thực hiện thao tác này.");
            }
            return userId;
        }

        /// <summary>
        /// API Lấy thông tin cá nhân (Profile) người dùng hiện tại (SCRUM-7 Sub-task 2.3)
        /// </summary>
        [HttpGet("profile")]
        [ProducesResponseType(typeof(ApiResponse<UserProfileDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetProfile()
        {
            var userId = GetCurrentUserId();
            var result = await _userService.GetProfileAsync(userId);
            return Ok(result);
        }

        /// <summary>
        /// API Cập nhật thông tin cá nhân (Họ tên, SĐT, Avatar) (SCRUM-7 Sub-task 2.3)
        /// </summary>
        [HttpPut("profile")]
        [ProducesResponseType(typeof(ApiResponse<UserProfileDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail("Dữ liệu không hợp lệ", 400, ModelState));
            }

            var userId = GetCurrentUserId();
            var result = await _userService.UpdateProfileAsync(userId, request);
            return Ok(result);
        }

        /// <summary>
        /// API Đổi mật khẩu
        /// </summary>
        [HttpPost("change-password")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail("Dữ liệu không hợp lệ", 400, ModelState));
            }

            var userId = GetCurrentUserId();
            var result = await _userService.ChangePasswordAsync(userId, request);
            return Ok(result);
        }
    }
}
