using System.Security.Claims;
using ClonEbay_CoreAPI.Common.Models;
using ClonEbay_CoreAPI.DTOs.ReturnRequest;
using ClonEbay_CoreAPI.Exceptions;
using ClonEbay_CoreAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClonEbay_CoreAPI.Controllers
{
    [ApiController]
    [Route("api/return-requests")]
    [Authorize]
    public class ReturnRequestController : ControllerBase
    {
        private readonly IReturnRequestService _returnRequestService;

        public ReturnRequestController(IReturnRequestService returnRequestService)
        {
            _returnRequestService = returnRequestService;
        }

        // ─── GET /api/return-requests ─────────────────────────────────────────
        /// <summary>Buyer xem danh sách yêu cầu hoàn trả của mình.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<ReturnRequestDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyRequests()
        {
            return Ok(await _returnRequestService.GetMyRequestsAsync(GetCurrentUserId()));
        }

        // ─── GET /api/return-requests/seller ──────────────────────────────────
        /// <summary>Seller xem các yêu cầu hoàn trả đối với các sản phẩm của shop mình.</summary>
        [HttpGet("seller")]
        [Authorize(Roles = "Seller,Admin,SELLER,ADMIN,seller,admin")]
        [ProducesResponseType(typeof(ApiResponse<List<ReturnRequestDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSellerRequests()
        {
            return Ok(await _returnRequestService.GetSellerRequestsAsync(GetCurrentUserId()));
        }

        // ─── GET /api/return-requests/{id} ───────────────────────────────────
        /// <summary>Xem chi tiết một yêu cầu hoàn trả.</summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<ReturnRequestDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetById(int id)
        {
            return Ok(await _returnRequestService.GetByIdAsync(id, GetCurrentUserId(), GetCurrentUserRole()));
        }

        // ─── POST /api/return-requests ────────────────────────────────────────
        /// <summary>Buyer gửi yêu cầu hoàn trả mới.</summary>
        [HttpPost]
        [Authorize(Roles = "Buyer,BUYER,buyer,User,USER,user")]
        [ProducesResponseType(typeof(ApiResponse<List<ReturnRequestDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Create([FromBody] CreateReturnRequestDto dto)
        {
            return Ok(await _returnRequestService.CreateAsync(GetCurrentUserId(), dto));
        }

        // ─── PUT /api/return-requests/{id}/cancel ────────────────────────────
        /// <summary>Buyer huỷ yêu cầu hoàn trả của mình.</summary>
        [HttpPut("{id:int}/cancel")]
        [Authorize(Roles = "Buyer,BUYER,buyer,User,USER,user")]
        [ProducesResponseType(typeof(ApiResponse<ReturnRequestDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Cancel(int id)
        {
            return Ok(await _returnRequestService.CancelAsync(id, GetCurrentUserId()));
        }

        // ─── PUT /api/return-requests/{id}/status ────────────────────────────
        /// <summary>Seller hoặc Admin duyệt / từ chối / hoàn tiền cho yêu cầu hoàn trả.</summary>
        [HttpPut("{id:int}/status")]
        [Authorize(Roles = "Seller,Admin,SELLER,ADMIN,seller,admin")]
        [ProducesResponseType(typeof(ApiResponse<ReturnRequestDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateReturnRequestStatusDto dto)
        {
            return Ok(await _returnRequestService.UpdateStatusAsync(id, dto, GetCurrentUserId(), GetCurrentUserRole()));
        }

        // ─── PUT /api/return-requests/{id}/tracking ──────────────────────────
        /// <summary>Buyer cập nhật mã vận chuyển gửi trả hàng.</summary>
        [HttpPut("{id:int}/tracking")]
        [Authorize(Roles = "Buyer,BUYER,buyer,User,USER,user")]
        [ProducesResponseType(typeof(ApiResponse<ReturnRequestDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateTracking(int id, [FromBody] UpdateReturnTrackingDto dto)
        {
            return Ok(await _returnRequestService.UpdateTrackingAsync(id, GetCurrentUserId(), dto));
        }

        // ─── PUT /api/return-requests/{id}/confirm-returned ─────────────────
        /// <summary>Seller / Admin xác nhận đã nhận lại hàng và hoàn tiền.</summary>
        [HttpPut("{id:int}/confirm-returned")]
        [Authorize(Roles = "Seller,Admin,SELLER,ADMIN,seller,admin")]
        [ProducesResponseType(typeof(ApiResponse<ReturnRequestDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ConfirmReturned(int id)
        {
            return Ok(await _returnRequestService.ConfirmItemReturnedAsync(id, GetCurrentUserId(), GetCurrentUserRole()));
        }

        // ─── PUT /api/return-requests/{id}/escalate ─────────────────────────
        /// <summary>Buyer yêu cầu Admin can thiệp (Ask eBay to step in).</summary>
        [HttpPut("{id:int}/escalate")]
        [Authorize(Roles = "Buyer,BUYER,buyer,User,USER,user")]
        [ProducesResponseType(typeof(ApiResponse<ReturnRequestDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Escalate(int id, [FromBody] EscalateReturnDto dto)
        {
            return Ok(await _returnRequestService.EscalateToAdminAsync(id, GetCurrentUserId(), dto));
        }

        // ─── Private helpers ──────────────────────────────────────────────────

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedException("Vui lòng đăng nhập để thực hiện chức năng này.");
            }
            return userId;
        }

        private string GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        }
    }
}
