using System.Security.Claims;
using ClonEbay_CoreAPI.DTOs.Commerce;
using ClonEbay_CoreAPI.Exceptions;
using ClonEbay_CoreAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace ClonEbay_CoreAPI.Controllers;
[ApiController, Authorize]
[Route("api/[controller]")]
public sealed class CartController(ICartService cartService) : ControllerBase
{
    private int UserId() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : throw new UnauthorizedException();
    [HttpGet] public async Task<IActionResult> Get() => Ok(await cartService.GetAsync(UserId()));
    [HttpPost("items")] public async Task<IActionResult> Add(AddCartItemRequestDto request) => Ok(await cartService.AddAsync(UserId(), request));
    [HttpPut("items/{productId:int}")] public async Task<IActionResult> Update(int productId, UpdateCartItemRequestDto request) => Ok(await cartService.UpdateAsync(UserId(), productId, request));
    [HttpDelete("items/{productId:int}")] public async Task<IActionResult> Remove(int productId) => Ok(await cartService.RemoveAsync(UserId(), productId));
    [HttpPost("merge")] public async Task<IActionResult> Merge(IEnumerable<AddCartItemRequestDto> items) => Ok(await cartService.MergeAsync(UserId(), items));
}
