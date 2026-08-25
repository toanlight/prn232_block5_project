using System.Security.Claims;
using ClonEbay_CoreAPI.DTOs.Auction;
using ClonEbay_CoreAPI.Exceptions;
using ClonEbay_CoreAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClonEbay_CoreAPI.Controllers;

[ApiController]
[Route("api/auctions")]
public sealed class AuctionsController(IAuctionService auctionService) : ControllerBase
{
    [HttpGet("product/{productId:int}")]
    public async Task<IActionResult> GetByProduct(int productId, CancellationToken cancellationToken) =>
        Ok(await auctionService.GetByProductAsync(productId, OptionalUserId(), cancellationToken));

    [Authorize]
    [HttpPost("product/{productId:int}/bids")]
    public async Task<IActionResult> PlaceBid(
        int productId,
        PlaceBidRequestDto request,
        CancellationToken cancellationToken) =>
        Ok(await auctionService.PlaceBidAsync(productId, RequiredUserId(), request, cancellationToken));

    private int? OptionalUserId() =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    private int RequiredUserId() => OptionalUserId() ?? throw new UnauthorizedException();
}
