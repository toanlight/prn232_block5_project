using System.Security.Claims;
using ClonEbay_CoreAPI.Common.Models;
using ClonEbay_CoreAPI.DTOs.Auth;
using ClonEbay_CoreAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClonEbay_CoreAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// API Đăng ký tài khoản người dùng (SCRUM-6 Sub-task 1.2 & 1.3)
        /// </summary>
        [HttpPost("register")]
        [ProducesResponseType(typeof(ApiResponse<UserInfoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail("Dữ liệu gửi lên không hợp lệ", 400, ModelState));
            }

            var result = await _authService.RegisterAsync(request);
            return Ok(result);
        }

        /// <summary>
        /// API Xác thực Email bằng mã OTP (SCRUM-6 Sub-task 1.3 & SCRUM-23)
        /// </summary>
        [HttpPost("verify-otp")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail("Dữ liệu không hợp lệ", 400, ModelState));
            }

            var result = await _authService.VerifyOtpAsync(request);
            return Ok(result);
        }

        /// <summary>
        /// API Gửi lại mã OTP xác nhận
        /// </summary>
        [HttpPost("resend-otp")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResendOtp([FromBody] ResendOtpRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail("Dữ liệu không hợp lệ", 400, ModelState));
            }

            var result = await _authService.ResendOtpAsync(request);
            return Ok(result);
        }

        /// <summary>
        /// API Đăng nhập cấp Access Token (JWT) & Refresh Token (SCRUM-7 Sub-task 2.1)
        /// </summary>
        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail("Dữ liệu không hợp lệ", 400, ModelState));
            }

            var result = await _authService.LoginAsync(request);
            return Ok(result);
        }

        /// <summary>
        /// API Làm mới Access Token bằng Refresh Token (SCRUM-7 Sub-task 2.1)
        /// </summary>
        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail("Dữ liệu không hợp lệ", 400, ModelState));
            }

            var result = await _authService.RefreshTokenAsync(request);
            return Ok(result);
        }

        /// <summary>
        /// API Đăng xuất (Vô hiệu hóa Refresh Token)
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Logout()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out var userId))
            {
                await _authService.LogoutAsync(userId);
            }
            return Ok(ApiResponse<bool>.Ok(true, "Đăng xuất thành công"));
        }
    }
}
