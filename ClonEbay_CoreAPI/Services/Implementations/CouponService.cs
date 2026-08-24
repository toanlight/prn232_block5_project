using ClonEbay_CoreAPI.Common.Models;
using ClonEbay_CoreAPI.DTOs.Coupon;
using ClonEbay_CoreAPI.DTOs.Notification;
using ClonEbay_CoreAPI.Exceptions;
using ClonEbay_CoreAPI.Models;
using ClonEbay_CoreAPI.Repositories.Interfaces;
using ClonEbay_CoreAPI.Services.Interfaces;

namespace ClonEbay_CoreAPI.Services.Implementations
{
    public class CouponService : ICouponService
    {
        private readonly ICouponRepository _couponRepo;
        private readonly IGenericRepository<Product> _productRepo;
        private readonly INotificationService _notificationService;

        public CouponService(
            ICouponRepository couponRepo,
            IGenericRepository<Product> productRepo,
            INotificationService notificationService)
        {
            _couponRepo = couponRepo;
            _productRepo = productRepo;
            _notificationService = notificationService;
        }

        // ─── GET: Danh sách coupon khả dụng ────────────────────────────────────
        public async Task<ApiResponse<List<CouponDto>>> GetActiveCouponsAsync()
        {
            var coupons = await _couponRepo.GetActiveCouponsAsync();
            return ApiResponse<List<CouponDto>>.Ok(coupons.Select(ToDto).ToList());
        }

        // ─── POST: Áp dụng mã giảm giá ─────────────────────────────────────────
        public async Task<ApiResponse<ApplyCouponResponseDto>> ApplyCouponAsync(ApplyCouponRequestDto dto)
        {
            var code = dto.Code.Trim().ToUpper();

            // 1. Kiểm tra coupon có tồn tại và đúng ProductId không
            var coupon = await _couponRepo.GetByCodeAndProductAsync(code, dto.ProductId)
                         ?? throw new BadRequestException($"Mã giảm giá '{dto.Code}' không tồn tại hoặc không áp dụng cho sản phẩm này.");

            var now = DateTime.UtcNow;

            // 2. Kiểm tra ngày bắt đầu
            if (coupon.StartDate.HasValue && coupon.StartDate.Value > now)
            {
                throw new BadRequestException($"Mã giảm giá '{coupon.Code}' chưa đến đợt sử dụng (Bắt đầu từ: {coupon.StartDate:dd/MM/yyyy HH:mm}).");
            }

            // 3. Kiểm tra ngày kết thúc
            if (coupon.EndDate.HasValue && coupon.EndDate.Value < now)
            {
                throw new BadRequestException($"Mã giảm giá '{coupon.Code}' đã hết hạn sử dụng.");
            }

            // 4. Kiểm tra số lần sử dụng tối đa
            if (coupon.MaxUsage.HasValue && coupon.MaxUsage.Value <= 0)
            {
                throw new BadRequestException($"Mã giảm giá '{coupon.Code}' đã hết lượt sử dụng.");
            }

            // 5. Tính toán số tiền được giảm
            var discountPercent = coupon.DiscountPercent ?? 0;
            var discountAmount = Math.Round(dto.OriginalPrice * (discountPercent / 100m), 2);
            var finalPrice = Math.Max(0, dto.OriginalPrice - discountAmount);

            // 6. Giảm lượt sử dụng MaxUsage đi 1 khi áp dụng thành công
            if (coupon.MaxUsage.HasValue)
            {
                coupon.MaxUsage -= 1;
            }

            await _couponRepo.SaveChangesAsync();

            var response = new ApplyCouponResponseDto
            {
                Code = coupon.Code ?? string.Empty,
                DiscountPercent = discountPercent,
                OriginalPrice = dto.OriginalPrice,
                DiscountAmount = discountAmount,
                FinalPrice = finalPrice,
                Message = $"Áp dụng mã giảm giá thành công! Giảm {discountPercent}% ({discountAmount:N0} VNĐ)."
            };

            return ApiResponse<ApplyCouponResponseDto>.Ok(response, response.Message);
        }

        // ─── POST: Tạo mã giảm giá mới (Seller/Admin) ──────────────────────────
        public async Task<ApiResponse<CouponDto>> CreateCouponAsync(CreateCouponDto dto)
        {
            var code = dto.Code.Trim().ToUpper();

            // 1. Kiểm tra trùng mã
            var existing = await _couponRepo.GetByCodeAsync(code);
            if (existing != null)
            {
                throw new BadRequestException($"Mã giảm giá '{code}' đã tồn tại trong hệ thống.");
            }

            // 2. Kiểm tra sản phẩm tồn tại
            var product = await _productRepo.GetByIdAsync(dto.ProductId)
                          ?? throw new NotFoundException("Không tìm thấy sản phẩm được áp dụng.");

            // 3. Kiểm tra ngày hợp lệ
            if (dto.StartDate.HasValue && dto.EndDate.HasValue && dto.StartDate.Value > dto.EndDate.Value)
            {
                throw new BadRequestException("Ngày bắt đầu không được lớn hơn ngày kết thúc.");
            }

            var coupon = new Coupon
            {
                Code = code,
                DiscountPercent = dto.DiscountPercent,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                MaxUsage = dto.MaxUsage,
                ProductId = dto.ProductId
            };

            await _couponRepo.AddAsync(coupon);
            await _couponRepo.SaveChangesAsync();

            var createdCoupon = await _couponRepo.GetByIdAsync(coupon.Id);

            // Gửi thông báo Real-time qua SignalR cho các Buyer
            _ = Task.Run(() => _notificationService.SendPromotionNotificationAsync(new PromotionNotificationDto
            {
                CouponId = coupon.Id,
                Code = coupon.Code ?? string.Empty,
                DiscountPercent = dto.DiscountPercent,
                ProductId = dto.ProductId,
                ProductTitle = product.Title ?? string.Empty,
                Message = $"Mã giảm giá '{coupon.Code}' vừa ra mắt! Giảm {dto.DiscountPercent}% cho sản phẩm {product.Title}."
            }));

            return ApiResponse<CouponDto>.Ok(ToDto(createdCoupon ?? coupon), "Tạo mã giảm giá mới thành công.");
        }

        // ─── PUT: Cập nhật mã giảm giá (Seller/Admin) ──────────────────────────
        public async Task<ApiResponse<CouponDto>> UpdateCouponAsync(int id, UpdateCouponDto dto)
        {
            var coupon = await _couponRepo.GetByIdAsync(id)
                         ?? throw new NotFoundException("Không tìm thấy mã giảm giá.");

            var newCode = dto.Code.Trim().ToUpper();

            // Check trùng mã nếu đổi code
            if (!string.Equals(coupon.Code, newCode, StringComparison.OrdinalIgnoreCase))
            {
                var existing = await _couponRepo.GetByCodeAsync(newCode);
                if (existing != null)
                {
                    throw new BadRequestException($"Mã giảm giá '{newCode}' đã tồn tại trong hệ thống.");
                }
            }

            // Check sản phẩm tồn tại
            var product = await _productRepo.GetByIdAsync(dto.ProductId)
                          ?? throw new NotFoundException("Không tìm thấy sản phẩm được áp dụng.");

            if (dto.StartDate.HasValue && dto.EndDate.HasValue && dto.StartDate.Value > dto.EndDate.Value)
            {
                throw new BadRequestException("Ngày bắt đầu không được lớn hơn ngày kết thúc.");
            }

            coupon.Code = newCode;
            coupon.DiscountPercent = dto.DiscountPercent;
            coupon.StartDate = dto.StartDate;
            coupon.EndDate = dto.EndDate;
            coupon.MaxUsage = dto.MaxUsage;
            coupon.ProductId = dto.ProductId;

            await _couponRepo.SaveChangesAsync();

            return ApiResponse<CouponDto>.Ok(ToDto(coupon), "Cập nhật mã giảm giá thành công.");
        }

        // ─── DELETE: Xóa mã giảm giá (Seller/Admin) ─────────────────────────────
        public async Task<ApiResponse<bool>> DeleteCouponAsync(int id)
        {
            var coupon = await _couponRepo.GetByIdAsync(id)
                         ?? throw new NotFoundException("Không tìm thấy mã giảm giá.");

            _couponRepo.Delete(coupon);
            await _couponRepo.SaveChangesAsync();

            return ApiResponse<bool>.Ok(true, "Xóa mã giảm giá thành công.");
        }

        // ─── Private helpers ────────────────────────────────────────────────────
        private static CouponDto ToDto(Coupon c)
        {
            return new CouponDto
            {
                Id = c.Id,
                Code = c.Code ?? string.Empty,
                DiscountPercent = c.DiscountPercent ?? 0,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                MaxUsage = c.MaxUsage ?? 0,
                ProductId = c.ProductId,
                ProductTitle = c.Product?.Title
            };
        }
    }
}
