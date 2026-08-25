using System.ComponentModel.DataAnnotations;

namespace ClonEbay_CoreAPI.DTOs.Auction;

public sealed class AuctionDetailDto
{
    public int ProductId { get; init; }
    public decimal StartingPrice { get; init; }
    public decimal CurrentPrice { get; init; }
    public decimal MinimumNextBid { get; init; }
    public decimal BidIncrement { get; init; }
    public DateTime EndTime { get; init; }
    public string Status { get; init; } = string.Empty;
    public int BidCount { get; init; }
    public bool IsCurrentUserLeading { get; init; }
    public decimal? CurrentUserBid { get; init; }
    public int? OrderId { get; init; }
    public IReadOnlyList<AuctionBidDto> RecentBids { get; init; } = [];
}

public sealed class AuctionBidDto
{
    public int Id { get; init; }
    public string Bidder { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public DateTime BidTime { get; init; }
    public bool IsCurrentUser { get; init; }
}

public sealed class PlaceBidRequestDto
{
    [Range(typeof(decimal), "0.01", "99949999.99", ErrorMessage = "Giá đặt phải từ 0,01 đến 99.949.999,99.")]
    public decimal BidAmount { get; init; }
}

public sealed class AuctionRealtimeDto
{
    public int ProductId { get; init; }
    public decimal CurrentPrice { get; init; }
    public decimal MinimumNextBid { get; init; }
    public int BidCount { get; init; }
    public DateTime EndTime { get; init; }
    public string Status { get; init; } = string.Empty;
}

public sealed class AuctionNotificationDto
{
    public int ProductId { get; init; }
    public string Type { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public int? OrderId { get; init; }
}
