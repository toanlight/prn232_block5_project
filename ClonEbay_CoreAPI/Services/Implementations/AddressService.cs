using ClonEbay_CoreAPI.Common.Models;
using ClonEbay_CoreAPI.DTOs.Address;
using ClonEbay_CoreAPI.Exceptions;
using ClonEbay_CoreAPI.Models;
using ClonEbay_CoreAPI.Repositories.Interfaces;
using ClonEbay_CoreAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClonEbay_CoreAPI.Services.Implementations
{
    public class AddressService : IAddressService
    {
        private readonly IAddressRepository _addressRepository;

        public AddressService(IAddressRepository addressRepository)
        {
            _addressRepository = addressRepository;
        }

        public async Task<ApiResponse<List<AddressDto>>> GetAllAsync(int userId)
        {
            var addresses = await _addressRepository.GetByUserIdAsync(userId);
            return ApiResponse<List<AddressDto>>.Ok(addresses.Select(ToDto).ToList());
        }

        public async Task<ApiResponse<AddressDto>> GetByIdAsync(int userId, int addressId)
        {
            var address = await GetOwnedAddressAsync(userId, addressId);
            return ApiResponse<AddressDto>.Ok(ToDto(address));
        }

        public async Task<ApiResponse<AddressDto>> CreateAsync(int userId, SaveAddressRequestDto request)
        {
            var isFirstAddress = !await _addressRepository.AnyForUserAsync(userId);
            var shouldBeDefault = isFirstAddress || request.IsDefault;

            await using var transaction = await _addressRepository.BeginTransactionAsync();
            try
            {
                if (shouldBeDefault && !isFirstAddress)
                {
                    await _addressRepository.ClearDefaultAsync(userId);
                    await _addressRepository.SaveChangesAsync();
                }

                var address = new Address
                {
                    UserId = userId,
                    IsDefault = shouldBeDefault
                };
                ApplyRequest(address, request);

                await _addressRepository.AddAsync(address);
                await _addressRepository.SaveChangesAsync();
                await transaction.CommitAsync();

                return ApiResponse<AddressDto>.Ok(ToDto(address), "Thêm địa chỉ giao hàng thành công.");
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync();
                throw new BadRequestException("Không thể lưu địa chỉ. Vui lòng kiểm tra dữ liệu và thử lại.");
            }
        }

        public async Task<ApiResponse<AddressDto>> UpdateAsync(int userId, int addressId, SaveAddressRequestDto request)
        {
            var address = await GetOwnedAddressAsync(userId, addressId);
            var setAsDefault = request.IsDefault && !address.IsDefault;

            await using var transaction = await _addressRepository.BeginTransactionAsync();
            try
            {
                if (setAsDefault)
                {
                    await _addressRepository.ClearDefaultAsync(userId, addressId);
                    await _addressRepository.SaveChangesAsync();
                    address.IsDefault = true;
                }

                ApplyRequest(address, request);
                _addressRepository.Update(address);
                await _addressRepository.SaveChangesAsync();
                await transaction.CommitAsync();

                return ApiResponse<AddressDto>.Ok(ToDto(address), "Cập nhật địa chỉ giao hàng thành công.");
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync();
                throw new BadRequestException("Không thể cập nhật địa chỉ. Vui lòng kiểm tra dữ liệu và thử lại.");
            }
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int userId, int addressId)
        {
            var address = await GetOwnedAddressAsync(userId, addressId);
            var wasDefault = address.IsDefault;

            await using var transaction = await _addressRepository.BeginTransactionAsync();
            try
            {
                _addressRepository.Delete(address);
                await _addressRepository.SaveChangesAsync();

                if (wasDefault)
                {
                    var replacement = await _addressRepository.GetFirstForUserAsync(userId);
                    if (replacement != null)
                    {
                        replacement.IsDefault = true;
                        _addressRepository.Update(replacement);
                        await _addressRepository.SaveChangesAsync();
                    }
                }

                await transaction.CommitAsync();
                return ApiResponse<bool>.Ok(true, "Xóa địa chỉ giao hàng thành công.");
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync();
                throw new BadRequestException("Không thể xóa địa chỉ đang được sử dụng trong đơn hàng.");
            }
        }

        public async Task<ApiResponse<AddressDto>> SetDefaultAsync(int userId, int addressId)
        {
            var address = await GetOwnedAddressAsync(userId, addressId);
            if (address.IsDefault)
            {
                return ApiResponse<AddressDto>.Ok(ToDto(address), "Địa chỉ này đã là địa chỉ mặc định.");
            }

            await using var transaction = await _addressRepository.BeginTransactionAsync();
            try
            {
                await _addressRepository.ClearDefaultAsync(userId, addressId);
                await _addressRepository.SaveChangesAsync();

                address.IsDefault = true;
                _addressRepository.Update(address);
                await _addressRepository.SaveChangesAsync();
                await transaction.CommitAsync();

                return ApiResponse<AddressDto>.Ok(ToDto(address), "Đã chọn địa chỉ giao hàng mặc định.");
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync();
                throw new BadRequestException("Không thể đặt địa chỉ mặc định. Vui lòng thử lại.");
            }
        }

        private async Task<Address> GetOwnedAddressAsync(int userId, int addressId)
        {
            var address = await _addressRepository.GetByIdForUserAsync(addressId, userId);
            return address ?? throw new NotFoundException("Không tìm thấy địa chỉ giao hàng.");
        }

        private static void ApplyRequest(Address address, SaveAddressRequestDto request)
        {
            address.FullName = request.FullName.Trim();
            address.Phone = request.Phone.Trim();
            address.Street = request.Street.Trim();
            address.City = request.City.Trim();
            address.State = string.IsNullOrWhiteSpace(request.State) ? null : request.State.Trim();
            address.Country = request.Country.Trim();
            address.PostalCode = string.IsNullOrWhiteSpace(request.PostalCode) ? null : request.PostalCode.Trim();
        }

        private static AddressDto ToDto(Address address)
        {
            return new AddressDto
            {
                Id = address.Id,
                FullName = address.FullName ?? string.Empty,
                Phone = address.Phone ?? string.Empty,
                Street = address.Street ?? string.Empty,
                City = address.City ?? string.Empty,
                State = address.State,
                Country = address.Country ?? string.Empty,
                PostalCode = address.PostalCode,
                IsDefault = address.IsDefault
            };
        }
    }
}
