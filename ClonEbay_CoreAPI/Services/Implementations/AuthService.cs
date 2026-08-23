using System.Security.Claims;
using System.Security.Cryptography;
using BCrypt.Net;
using ClonEbay_CoreAPI.Common.Models;
using ClonEbay_CoreAPI.DTOs.Auth;
using ClonEbay_CoreAPI.Exceptions;
using ClonEbay_CoreAPI.Models;
using ClonEbay_CoreAPI.Repositories.Interfaces;
using ClonEbay_CoreAPI.Services.Interfaces;

namespace ClonEbay_CoreAPI.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUserRepository userRepository,
            IJwtService jwtService,
            IEmailService emailService,
            IConfiguration configuration,
            ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<ApiResponse<UserInfoDto>> RegisterAsync(RegisterRequestDto request)
        {
            // 1. Kiểm tra Email đã tồn tại qua Repository
            var emailExists = await _userRepository.ExistsByEmailAsync(request.Email);
            if (emailExists)
            {
                throw new BadRequestException("Địa chỉ email này đã được sử dụng. Vui lòng chọn email khác.");
            }

            // 2. Kiểm tra Username đã tồn tại qua Repository
            var usernameExists = await _userRepository.ExistsByUsernameAsync(request.Username);
            if (usernameExists)
            {
                throw new BadRequestException("Tên đăng nhập này đã được sử dụng. Vui lòng chọn tên khác.");
            }

            // 3. Hash mật khẩu bằng BCrypt
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12);

            // 4. Sinh mã OTP 6 số
            var otpCode = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
            var otpExpiry = DateTime.UtcNow.AddMinutes(10);

            // 5. Tạo User mới và lưu qua Repository
            var user = new User
            {
                Username = request.Username.Trim(),
                Email = request.Email.Trim().ToLower(),
                Password = hashedPassword,
                FullName = request.FullName?.Trim() ?? request.Username.Trim(),
                Phone = request.Phone?.Trim(),
                Role = "User",
                IsEmailVerified = false,
                VerificationCode = otpCode,
                VerificationExpiry = otpExpiry,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            // 6. Gửi email xác nhận
            await _emailService.SendVerificationEmailAsync(user.Email, user.Username, otpCode);

            var userInfo = new UserInfoDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                Phone = user.Phone,
                Role = user.Role,
                AvatarUrl = user.AvatarUrl,
                IsEmailVerified = user.IsEmailVerified
            };

            return ApiResponse<UserInfoDto>.Ok(userInfo, "Đăng ký tài khoản thành công! Vui lòng kiểm tra email để lấy mã OTP xác thực.");
        }

        public async Task<ApiResponse<bool>> VerifyOtpAsync(VerifyOtpRequestDto request)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null)
            {
                throw new NotFoundException("Không tìm thấy tài khoản với email này.");
            }

            if (user.IsEmailVerified)
            {
                return ApiResponse<bool>.Ok(true, "Tài khoản của bạn đã được kích hoạt trước đó.");
            }

            if (string.IsNullOrEmpty(user.VerificationCode) || user.VerificationCode != request.OtpCode.Trim())
            {
                throw new BadRequestException("Mã OTP không chính xác. Vui lòng kiểm tra lại.");
            }

            if (user.VerificationExpiry.HasValue && user.VerificationExpiry.Value < DateTime.UtcNow)
            {
                throw new BadRequestException("Mã OTP đã hết hạn. Vui lòng yêu cầu gửi lại mã mới.");
            }

            // Kích hoạt tài khoản thành công
            user.IsEmailVerified = true;
            user.VerificationCode = null;
            user.VerificationExpiry = null;
            user.UpdatedAt = DateTime.UtcNow;

            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();

            return ApiResponse<bool>.Ok(true, "Xác thực email thành công! Bạn có thể đăng nhập ngay bây giờ.");
        }

        public async Task<ApiResponse<bool>> ResendOtpAsync(ResendOtpRequestDto request)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null)
            {
                throw new NotFoundException("Không tìm thấy tài khoản với email này.");
            }

            if (user.IsEmailVerified)
            {
                return ApiResponse<bool>.Ok(true, "Tài khoản của bạn đã được kích hoạt.");
            }

            // Tạo mã OTP mới
            var otpCode = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
            user.VerificationCode = otpCode;
            user.VerificationExpiry = DateTime.UtcNow.AddMinutes(10);
            user.UpdatedAt = DateTime.UtcNow;

            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();

            if (string.IsNullOrEmpty(user.Email))
            {
                throw new BadRequestException("Tài khoản chưa có email hợp lệ.");
            }

            await _emailService.SendVerificationEmailAsync(user.Email, user.Username ?? "Quý khách", otpCode);

            return ApiResponse<bool>.Ok(true, "Mã OTP mới đã được gửi đến email của bạn.");
        }

        public async Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginRequestDto request)
        {
            // Tìm User bằng Username hoặc Email qua Repository
            var user = await _userRepository.GetByUsernameOrEmailAsync(request.UsernameOrEmail);

            if (user == null)
            {
                throw new UnauthorizedException("Tên đăng nhập hoặc mật khẩu không chính xác.");
            }

            // Kiểm tra mật khẩu bằng BCrypt
            bool isPasswordValid = false;
            try
            {
                isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.Password);
            }
            catch
            {
                isPasswordValid = user.Password == request.Password;
            }

            if (!isPasswordValid)
            {
                throw new UnauthorizedException("Tên đăng nhập hoặc mật khẩu không chính xác.");
            }

            // Kiểm tra kích hoạt email
            if (!user.IsEmailVerified)
            {
                throw new BadRequestException("Tài khoản chưa được kích hoạt email. Vui lòng xác thực email trước khi đăng nhập.");
            }

            // Sinh Access Token và Refresh Token
            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();
            var refreshExpiryDays = _configuration.GetValue<int>("JwtSettings:RefreshTokenExpirationDays", 7);
            var accessExpiryMinutes = _configuration.GetValue<int>("JwtSettings:AccessTokenExpirationMinutes", 120);

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(refreshExpiryDays);
            user.UpdatedAt = DateTime.UtcNow;

            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();

            var response = new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(accessExpiryMinutes),
                User = new UserInfoDto
                {
                    Id = user.Id,
                    Username = user.Username ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    FullName = user.FullName,
                    Phone = user.Phone,
                    Role = user.Role,
                    AvatarUrl = user.AvatarUrl,
                    IsEmailVerified = user.IsEmailVerified
                }
            };

            return ApiResponse<AuthResponseDto>.Ok(response, "Đăng nhập thành công!");
        }

        public async Task<ApiResponse<AuthResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto request)
        {
            var principal = _jwtService.GetPrincipalFromExpiredToken(request.AccessToken);
            if (principal == null)
            {
                throw new UnauthorizedException("Access Token không hợp lệ.");
            }

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedException("Token không chứa thông tin User hợp lệ.");
            }

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                throw new UnauthorizedException("Refresh Token không hợp lệ hoặc đã hết hạn. Vui lòng đăng nhập lại.");
            }

            var newAccessToken = _jwtService.GenerateAccessToken(user);
            var newRefreshToken = _jwtService.GenerateRefreshToken();
            var refreshExpiryDays = _configuration.GetValue<int>("JwtSettings:RefreshTokenExpirationDays", 7);
            var accessExpiryMinutes = _configuration.GetValue<int>("JwtSettings:AccessTokenExpirationMinutes", 120);

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(refreshExpiryDays);
            user.UpdatedAt = DateTime.UtcNow;

            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();

            var response = new AuthResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(accessExpiryMinutes),
                User = new UserInfoDto
                {
                    Id = user.Id,
                    Username = user.Username ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    FullName = user.FullName,
                    Phone = user.Phone,
                    Role = user.Role,
                    AvatarUrl = user.AvatarUrl,
                    IsEmailVerified = user.IsEmailVerified
                }
            };

            return ApiResponse<AuthResponseDto>.Ok(response, "Làm mới Token thành công!");
        }

        public async Task<ApiResponse<bool>> LogoutAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user != null)
            {
                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = null;
                user.UpdatedAt = DateTime.UtcNow;
                _userRepository.Update(user);
                await _userRepository.SaveChangesAsync();
            }

            return ApiResponse<bool>.Ok(true, "Đăng xuất thành công.");
        }
    }
}
