using ClonEbay_CoreAPI.Common.Models;
using ClonEbay_CoreAPI.DTOs.User;

namespace ClonEbay_CoreAPI.Services.Interfaces
{
    public interface IUserService
    {
        Task<ApiResponse<UserProfileDto>> GetProfileAsync(int userId);
        Task<ApiResponse<UserProfileDto>> UpdateProfileAsync(int userId, UpdateProfileRequestDto request);
        Task<ApiResponse<bool>> ChangePasswordAsync(int userId, ChangePasswordRequestDto request);
    }
}
