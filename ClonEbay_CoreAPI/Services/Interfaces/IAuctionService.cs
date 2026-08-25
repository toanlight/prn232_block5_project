using ClonEbay_CoreAPI.Common.Models;
using ClonEbay_CoreAPI.DTOs.Auction;

namespace ClonEbay_CoreAPI.Services.Interfaces;

public interface IAuctionService
{
    Task<ApiResponse<AuctionDetailDto>> GetByProductAsync(
        int productId,
        int? currentUserId = null,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<AuctionDetailDto>> PlaceBidAsync(
        int productId,
        int bidderId,
        PlaceBidRequestDto request,
        CancellationToken cancellationToken = default);

    Task<int> FinalizeDueAuctionsAsync(CancellationToken cancellationToken = default);
}
