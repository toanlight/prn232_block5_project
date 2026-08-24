using BCrypt.Net;
using ClonEbay_CoreAPI.Common.Models;
using ClonEbay_CoreAPI.DTOs.Address;
using ClonEbay_CoreAPI.DTOs.Order;
using ClonEbay_CoreAPI.DTOs.User;
using ClonEbay_CoreAPI.Exceptions;
using ClonEbay_CoreAPI.Repositories.Interfaces;
using ClonEbay_CoreAPI.Services.Interfaces;

namespace ClonEbay_CoreAPI.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository userRepository, ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<ApiResponse<UserProfileDto>> GetProfileAsync(int userId)
        {
            var user = await _userRepository.GetUserWithAddressesAsync(userId);

            if (user == null)
            {
                throw new NotFoundException("Không tìm thấy thông tin người dùng.");
            }

            var profile = new UserProfileDto
            {
                Id = user.Id,
                Username = user.Username ?? string.Empty,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName,
                Phone = user.Phone,
                Role = user.Role,
                AvatarUrl = user.AvatarUrl,
                IsEmailVerified = user.IsEmailVerified,
                CreatedAt = user.CreatedAt,
                Addresses = user.Addresses.Select(a => new AddressDto
                {
                    Id = a.Id,
                    FullName = a.FullName ?? string.Empty,
                    Phone = a.Phone ?? string.Empty,
                    Street = a.Street ?? string.Empty,
                    City = a.City ?? string.Empty,
                    State = a.State,
                    Country = a.Country ?? string.Empty,
                    PostalCode = a.PostalCode,
                    IsDefault = a.IsDefault
                }).ToList()
            };

            return ApiResponse<UserProfileDto>.Ok(profile);
        }

        public async Task<ApiResponse<UserProfileDto>> UpdateProfileAsync(int userId, UpdateProfileRequestDto request)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new NotFoundException("Không tìm thấy người dùng.");
            }

            user.FullName = request.FullName.Trim();
            user.Phone = request.Phone?.Trim();
            if (!string.IsNullOrEmpty(request.AvatarUrl))
            {
                user.AvatarUrl = request.AvatarUrl;
            }
            user.UpdatedAt = DateTime.UtcNow;

            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();

            return await GetProfileAsync(userId);
        }

        public async Task<ApiResponse<bool>> ChangePasswordAsync(int userId, ChangePasswordRequestDto request)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new NotFoundException("Không tìm thấy người dùng.");
            }

            bool isPasswordCorrect = false;
            try
            {
                isPasswordCorrect = BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.Password);
            }
            catch
            {
                isPasswordCorrect = user.Password == request.CurrentPassword;
            }

            if (!isPasswordCorrect)
            {
                throw new BadRequestException("Mật khẩu hiện tại không chính xác.");
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword, workFactor: 12);
            user.UpdatedAt = DateTime.UtcNow;

            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();

            return ApiResponse<bool>.Ok(true, "Đổi mật khẩu thành công!");
        }

        public async Task<ApiResponse<List<OrderDto>>> GetUserOrdersAsync(int userId)
        {
            var orders = await _userRepository.GetUserOrdersAsync(userId);
            var userReviews = await _userRepository.GetUserReviewedProductIdsAsync(userId);

            var result = orders.Select(o => new OrderDto
            {
                Id = o.Id,
                OrderDate = o.OrderDate,
                TotalPrice = o.TotalPrice ?? 0,
                Status = o.Status ?? "Pending",
                HasPendingReturnRequest = o.ReturnRequests.Any(r => r.Status == "Pending"),
                Items = o.OrderItems.Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    ProductId = oi.ProductId ?? 0,
                    ProductTitle = oi.Product?.Title ?? "Sản phẩm",
                    ImageUrl = oi.Product?.Images,
                    Quantity = oi.Quantity ?? 1,
                    UnitPrice = oi.UnitPrice ?? 0,
                    HasReviewed = userReviews.Contains(oi.ProductId ?? 0)
                }).ToList()
            }).ToList();

            return ApiResponse<List<OrderDto>>.Ok(result);
        }
    }
}
