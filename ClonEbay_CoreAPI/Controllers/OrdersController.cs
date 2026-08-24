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
    public async Task<IActionResult> GetCheckout([FromQuery] int? addressId = null) =>
        Ok(await orderService.GetCheckoutAsync(UserId(), addressId));

    [HttpPost("checkout")]
    public async Task<IActionResult> PlaceOrder(PlaceOrderRequestDto request) =>
        Ok(await orderService.PlaceOrderAsync(UserId(), request));

    private int UserId() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
        ? id
        : throw new UnauthorizedException();
}
