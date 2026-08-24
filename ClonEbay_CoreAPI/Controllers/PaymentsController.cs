using System.Security.Claims;
using ClonEbay_CoreAPI.DTOs.Payment;
using ClonEbay_CoreAPI.Exceptions;
using ClonEbay_CoreAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClonEbay_CoreAPI.Controllers;

[ApiController, Authorize]
[Route("api/[controller]")]
public sealed class PaymentsController(IPaymentService paymentService) : ControllerBase
{
    [HttpGet("paypal/{orderId:int}")]
    public async Task<IActionResult> GetPayPal(int orderId) =>
        Ok(await paymentService.GetPayPalPaymentAsync(UserId(), orderId));

    [HttpPost("paypal/{orderId:int}/simulate")]
    public async Task<IActionResult> SimulatePayPal(int orderId, SimulatePayPalRequestDto request) =>
        Ok(await paymentService.SimulatePayPalAsync(UserId(), orderId, request));

    private int UserId() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
        ? id
        : throw new UnauthorizedException();
}
