using ClonEbay_CoreAPI.Common.Models;
using ClonEbay_CoreAPI.DTOs.Address;

namespace ClonEbay_CoreAPI.Services.Interfaces
{
    public interface IAddressService
    {
        Task<ApiResponse<List<AddressDto>>> GetAllAsync(int userId);
        Task<ApiResponse<AddressDto>> GetByIdAsync(int userId, int addressId);
        Task<ApiResponse<AddressDto>> CreateAsync(int userId, SaveAddressRequestDto request);
        Task<ApiResponse<AddressDto>> UpdateAsync(int userId, int addressId, SaveAddressRequestDto request);
        Task<ApiResponse<bool>> DeleteAsync(int userId, int addressId);
        Task<ApiResponse<AddressDto>> SetDefaultAsync(int userId, int addressId);
    }
}
