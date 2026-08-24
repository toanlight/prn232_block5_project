using System.Security.Claims;
using ClonEbay_CoreAPI.DTOs.Order;
using ClonEbay_CoreAPI.Exceptions;
using ClonEbay_CoreAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClonEbay_CoreAPI.Controllers;

[ApiController, Authorize]
[Route("api/[controller]")]
public sealed class OrdersController(IOrderService orderService) : ControllerBase
{
    [HttpGet("checkout")]
    public async Task<IActionResult> GetCheckout([FromQuery] int? addressId = null, [FromQuery] string? coupons = null)
    {
        Dictionary<int, string>? appliedCoupons = null;
        if (!string.IsNullOrEmpty(coupons))
        {
            try
            {
                appliedCoupons = System.Text.Json.JsonSerializer.Deserialize<Dictionary<int, string>>(coupons);
            }
            catch { }
        }
        return Ok(await orderService.GetCheckoutAsync(UserId(), addressId, appliedCoupons));
    }

    [HttpPost("checkout")]
    public async Task<IActionResult> PlaceOrder(PlaceOrderRequestDto request) =>
        Ok(await orderService.PlaceOrderAsync(UserId(), request));

    [HttpGet("my-orders")]
    public async Task<IActionResult> GetMyOrders() =>
        Ok(await orderService.GetUserOrdersAsync(UserId()));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetOrderDetail(int id) =>
        Ok(await orderService.GetOrderDetailAsync(UserId(), id));

    [HttpPost("{id:int}/confirm-receipt")]
    public async Task<IActionResult> ConfirmReceipt(int id) =>
        Ok(await orderService.ConfirmReceiptAsync(UserId(), id));

    [HttpPost("{id:int}/confirm-received")]
    public async Task<IActionResult> ConfirmReceived(int id) =>
        Ok(await orderService.ConfirmReceivedAsync(UserId(), id));

    private int UserId() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
        ? id
        : throw new UnauthorizedException();
}
