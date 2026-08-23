using ClonEbay_CoreAPI.Common.Models;
using ClonEbay_CoreAPI.DTOs.Auth;

namespace ClonEbay_CoreAPI.Services.Interfaces
{
    public interface IAuthService
    {
        Task<ApiResponse<UserInfoDto>> RegisterAsync(RegisterRequestDto request);
        Task<ApiResponse<bool>> VerifyOtpAsync(VerifyOtpRequestDto request);
        Task<ApiResponse<bool>> ResendOtpAsync(ResendOtpRequestDto request);
        Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginRequestDto request);
        Task<ApiResponse<AuthResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto request);
        Task<ApiResponse<bool>> LogoutAsync(int userId);
    }
}
