using System.Security.Claims;
using ClonEbay_CoreAPI.Common.Models;
using ClonEbay_CoreAPI.DTOs.Address;
using ClonEbay_CoreAPI.Exceptions;
using ClonEbay_CoreAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClonEbay_CoreAPI.Controllers
{
    [ApiController]
    [Route("api/addresses")]
    [Authorize]
    public class AddressController : ControllerBase
    {
        private readonly IAddressService _addressService;

        public AddressController(IAddressService addressService)
        {
            _addressService = addressService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<AddressDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _addressService.GetAllAsync(GetCurrentUserId()));
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<AddressDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            return Ok(await _addressService.GetByIdAsync(GetCurrentUserId(), id));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<AddressDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Create([FromBody] SaveAddressRequestDto request)
        {
            return Ok(await _addressService.CreateAsync(GetCurrentUserId(), request));
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<AddressDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Update(int id, [FromBody] SaveAddressRequestDto request)
        {
            return Ok(await _addressService.UpdateAsync(GetCurrentUserId(), id, request));
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Delete(int id)
        {
            return Ok(await _addressService.DeleteAsync(GetCurrentUserId(), id));
        }

        [HttpPost("{id:int}/set-default")]
        [ProducesResponseType(typeof(ApiResponse<AddressDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SetDefault(int id)
        {
            return Ok(await _addressService.SetDefaultAsync(GetCurrentUserId(), id));
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedException("Vui lòng đăng nhập để quản lý địa chỉ giao hàng.");
            }

            return userId;
        }
    }
}
