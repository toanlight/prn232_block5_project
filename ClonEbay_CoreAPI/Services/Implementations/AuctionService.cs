using System.Data;
using ClonEbay_CoreAPI.Common.Models;
using ClonEbay_CoreAPI.DTOs.Auction;
using ClonEbay_CoreAPI.Exceptions;
using ClonEbay_CoreAPI.Hubs;
using ClonEbay_CoreAPI.Models;
using ClonEbay_CoreAPI.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ClonEbay_CoreAPI.Services.Implementations;

public sealed class AuctionService(
    CloneEbayDbContext context,
    IHubContext<AuctionHub> auctionHub,
    IHubContext<NotificationHub> notificationHub,
    ILogger<AuctionService> logger) : IAuctionService
{
    // Các cột tiền trong sql.sql là decimal(10,2). Chừa 50.000đ cho phí giao hàng
    // để totalPrice của đơn thắng đấu giá không vượt giới hạn của schema hiện có.
    private const decimal MaximumBidAmount = 99_949_999.99m;
    private const decimal MaximumDatabaseMoney = 99_999_999.99m;

    public async Task<ApiResponse<AuctionDetailDto>> GetByProductAsync(
        int productId,
        int? currentUserId = null,
        CancellationToken cancellationToken = default)
    {
        var product = await GetAuctionProductAsync(productId, asTracking: false, cancellationToken);

        if (product.AuctionEndTime <= DateTime.Now)
        {
            await FinalizeAuctionAsync(productId, cancellationToken);
            context.ChangeTracker.Clear();
            product = await GetAuctionProductAsync(productId, asTracking: false, cancellationToken);
        }

        return ApiResponse<AuctionDetailDto>.Ok(
            await BuildDetailAsync(product, currentUserId, cancellationToken));
    }

    public async Task<ApiResponse<AuctionDetailDto>> PlaceBidAsync(
        int productId,
        int bidderId,
        PlaceBidRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (request.BidAmount <= 0 || request.BidAmount > MaximumBidAmount)
        {
            throw new BadRequestException(
                $"Giá đặt phải từ 0,01 đến {MaximumBidAmount:N2} ₫ theo giới hạn decimal(10,2) của database.");
        }

        await using var transaction = await context.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        var product = await context.Products
            .FromSqlInterpolated($"SELECT * FROM dbo.Product WITH (UPDLOCK, HOLDLOCK, ROWLOCK) WHERE id = {productId}")
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy sản phẩm đấu giá.");

        EnsureAuctionProduct(product);

        if (product.AuctionEndTime <= DateTime.Now)
        {
            throw new BadRequestException("Phiên đấu giá đã kết thúc.");
        }

        if (product.SellerId == bidderId)
        {
            throw new BadRequestException("Người bán không được đặt giá cho sản phẩm của mình.");
        }

        if (!await context.Users.AnyAsync(item => item.Id == bidderId, cancellationToken))
        {
            throw new NotFoundException("Không tìm thấy người đặt giá.");
        }

        var startingPrice = product.Price!.Value;
        var validBids = await ValidBidQuery(product.Id, startingPrice)
            .OrderByDescending(item => item.Amount)
            .ThenBy(item => item.BidTime)
            .ThenBy(item => item.Id)
            .ToListAsync(cancellationToken);

        var previousWinnerId = validBids.FirstOrDefault()?.BidderId;
        var currentPrice = validBids.FirstOrDefault()?.Amount ?? startingPrice;
        var increment = BidIncrement(currentPrice);
        var minimumNextBid = validBids.Count == 0 ? startingPrice : currentPrice + increment;

        if (minimumNextBid > MaximumBidAmount)
        {
            throw new BadRequestException("Phiên đấu giá đã đạt giới hạn số tiền của database hiện tại.");
        }

        if (request.BidAmount < minimumNextBid)
        {
            throw new BadRequestException($"Giá đặt phải từ {minimumNextBid:N0} ₫ trở lên.");
        }

        context.Bids.Add(new Bid
        {
            ProductId = product.Id,
            BidderId = bidderId,
            Amount = request.BidAmount,
            BidTime = DateTime.Now
        });

        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        var detail = await BuildDetailAsync(product, bidderId, cancellationToken);
        await BroadcastUpdatedAsync(detail, cancellationToken);

        if (previousWinnerId.HasValue && previousWinnerId != bidderId)
        {
            await SendUserNotificationAsync(
                previousWinnerId.Value,
                new AuctionNotificationDto
                {
                    ProductId = product.Id,
                    Type = "OUTBID",
                    Message = $"Bạn đã bị vượt giá ở sản phẩm {product.Title}."
                },
                cancellationToken);
        }

        return ApiResponse<AuctionDetailDto>.Ok(
            detail,
            "Đặt giá thành công. Bạn đang dẫn đầu phiên đấu giá.");
    }

    public async Task<int> FinalizeDueAuctionsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.Now;
        var dueProductIds = await context.Products
            .AsNoTracking()
            .Where(product =>
                product.IsAuction == true &&
                product.Price.HasValue &&
                product.AuctionEndTime.HasValue &&
                product.AuctionEndTime <= now &&
                product.Bids.Any(bid =>
                    bid.BidderId.HasValue &&
                    bid.Amount.HasValue &&
                    bid.Amount >= product.Price) &&
                !product.OrderItems.Any(item =>
                    item.Order != null &&
                    item.Order.OrderDate >= product.AuctionEndTime))
            .OrderBy(product => product.AuctionEndTime)
            .Select(product => product.Id)
            .Take(50)
            .ToListAsync(cancellationToken);

        var finalized = 0;
        foreach (var productId in dueProductIds)
        {
            if (await FinalizeAuctionAsync(productId, cancellationToken))
            {
                finalized++;
            }
        }

        return finalized;
    }

    private async Task<bool> FinalizeAuctionAsync(int productId, CancellationToken cancellationToken)
    {
        context.ChangeTracker.Clear();
        await using var transaction = await context.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        var product = await context.Products
            .FromSqlInterpolated($"SELECT * FROM dbo.Product WITH (UPDLOCK, HOLDLOCK, ROWLOCK) WHERE id = {productId}")
            .SingleOrDefaultAsync(cancellationToken);

        if (product is null ||
            product.IsAuction != true ||
            !product.Price.HasValue ||
            !product.AuctionEndTime.HasValue ||
            product.AuctionEndTime > DateTime.Now)
        {
            await transaction.CommitAsync(cancellationToken);
            return false;
        }

        var startingPrice = product.Price.Value;
        var winnerBid = await ValidBidQuery(product.Id, startingPrice)
            .OrderByDescending(item => item.Amount)
            .ThenBy(item => item.BidTime)
            .ThenBy(item => item.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (winnerBid?.BidderId is null || winnerBid.Amount is null)
        {
            await transaction.CommitAsync(cancellationToken);
            return false;
        }

        var existingOrderId = await FindAuctionOrderIdAsync(
            product,
            winnerBid.BidderId.Value,
            cancellationToken);
        if (existingOrderId.HasValue)
        {
            await transaction.CommitAsync(cancellationToken);
            return false;
        }

        var inventory = await context.Inventories
            .FromSqlInterpolated($"SELECT * FROM dbo.Inventory WITH (UPDLOCK, HOLDLOCK, ROWLOCK) WHERE productId = {product.Id}")
            .OrderBy(item => item.Id)
            .FirstOrDefaultAsync(cancellationToken);
        if (inventory is null || (inventory.Quantity ?? 0) < 1)
        {
            await transaction.CommitAsync(cancellationToken);
            logger.LogWarning("Không thể tạo đơn đấu giá cho product {ProductId}: không đủ tồn kho.", product.Id);
            return false;
        }

        var address = await context.Addresses
            .Where(item => item.UserId == winnerBid.BidderId)
            .OrderByDescending(item => item.IsDefault)
            .ThenBy(item => item.Id)
            .FirstOrDefaultAsync(cancellationToken);
        var shippingFee = address is null ? 0m : CalculateShippingFee(address);
        var totalPrice = winnerBid.Amount.Value + shippingFee;
        if (totalPrice > MaximumDatabaseMoney)
        {
            await transaction.CommitAsync(cancellationToken);
            logger.LogWarning(
                "Không thể tạo đơn đấu giá cho product {ProductId}: tổng tiền {TotalPrice} vượt decimal(10,2).",
                product.Id,
                totalPrice);
            return false;
        }

        var now = DateTime.Now;
        var order = new OrderTable
        {
            BuyerId = winnerBid.BidderId,
            AddressId = address?.Id,
            OrderDate = now,
            TotalPrice = totalPrice,
            Status = "Pending",
            OrderItems =
            [
                new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = 1,
                    UnitPrice = winnerBid.Amount
                }
            ],
            Payments =
            [
                new Payment
                {
                    UserId = winnerBid.BidderId,
                    Amount = totalPrice,
                    Method = "PayPal",
                    Status = "Pending"
                }
            ],
            ShippingInfos =
            [
                new ShippingInfo
                {
                    Carrier = "Standard",
                    Status = "Preparing",
                    EstimatedArrival = now.AddDays(address is not null && IsHoChiMinhCity(address.City) ? 3 : 5)
                }
            ]
        };

        inventory.Quantity = (inventory.Quantity ?? 0) - 1;
        inventory.LastUpdated = now;
        context.OrderTables.Add(order);

        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        await BroadcastEndedAsync(
            product,
            "SOLD",
            "Phiên đấu giá đã kết thúc.",
            order.Id,
            cancellationToken);
        await SendUserNotificationAsync(
            winnerBid.BidderId.Value,
            new AuctionNotificationDto
            {
                ProductId = product.Id,
                Type = "AUCTION_WON",
                Message = $"Chúc mừng! Bạn đã thắng đấu giá {product.Title}.",
                OrderId = order.Id
            },
            cancellationToken);

        if (product.SellerId.HasValue)
        {
            await SendUserNotificationAsync(
                product.SellerId.Value,
                new AuctionNotificationDto
                {
                    ProductId = product.Id,
                    Type = "AUCTION_SOLD",
                    Message = $"Sản phẩm {product.Title} đã đấu giá thành công.",
                    OrderId = order.Id
                },
                cancellationToken);
        }

        return true;
    }

    private async Task<Product> GetAuctionProductAsync(
        int productId,
        bool asTracking,
        CancellationToken cancellationToken)
    {
        var query = context.Products.AsQueryable();
        if (!asTracking)
        {
            query = query.AsNoTracking();
        }

        var product = await query.SingleOrDefaultAsync(item => item.Id == productId, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy sản phẩm đấu giá.");
        EnsureAuctionProduct(product);
        return product;
    }

    private static void EnsureAuctionProduct(Product product)
    {
        if (product.IsAuction != true || !product.AuctionEndTime.HasValue)
        {
            throw new NotFoundException("Sản phẩm này không có phiên đấu giá.");
        }

        if (!product.Price.HasValue || product.Price <= 0)
        {
            throw new BadRequestException("Sản phẩm đấu giá chưa có giá khởi điểm hợp lệ.");
        }
    }

    private IQueryable<Bid> ValidBidQuery(int productId, decimal startingPrice) =>
        context.Bids.Where(item =>
            item.ProductId == productId &&
            item.BidderId.HasValue &&
            item.Amount.HasValue &&
            item.Amount >= startingPrice);

    private async Task<AuctionDetailDto> BuildDetailAsync(
        Product product,
        int? currentUserId,
        CancellationToken cancellationToken)
    {
        var startingPrice = product.Price!.Value;
        var bids = await ValidBidQuery(product.Id, startingPrice)
            .AsNoTracking()
            .Include(item => item.Bidder)
            .ToListAsync(cancellationToken);

        var winnerBid = bids
            .OrderByDescending(item => item.Amount)
            .ThenBy(item => item.BidTime)
            .ThenBy(item => item.Id)
            .FirstOrDefault();
        var currentPrice = winnerBid?.Amount ?? startingPrice;
        var currentWinnerId = winnerBid?.BidderId;
        var orderId = currentWinnerId.HasValue
            ? await FindAuctionOrderIdAsync(product, currentWinnerId.Value, cancellationToken)
            : null;
        var isActive = product.AuctionEndTime > DateTime.Now;

        var status = isActive
            ? "Active"
            : orderId.HasValue
                ? "Sold"
                : winnerBid is null
                    ? "Unsold"
                    : "Ended";

        var currentUserBid = currentUserId.HasValue
            ? bids.Where(item => item.BidderId == currentUserId)
                .Select(item => item.Amount)
                .Max()
            : null;

        return new AuctionDetailDto
        {
            ProductId = product.Id,
            StartingPrice = startingPrice,
            CurrentPrice = currentPrice,
            MinimumNextBid = bids.Count == 0 ? startingPrice : currentPrice + BidIncrement(currentPrice),
            BidIncrement = BidIncrement(currentPrice),
            EndTime = product.AuctionEndTime!.Value,
            Status = status,
            BidCount = bids.Count,
            IsCurrentUserLeading = currentUserId.HasValue && currentWinnerId == currentUserId,
            CurrentUserBid = currentUserBid,
            OrderId = currentUserId.HasValue && currentWinnerId == currentUserId ? orderId : null,
            RecentBids = bids
                .Where(item => item.BidTime.HasValue)
                .OrderByDescending(item => item.BidTime)
                .ThenByDescending(item => item.Id)
                .Take(10)
                .Select(item => new AuctionBidDto
                {
                    Id = item.Id,
                    Bidder = item.BidderId == currentUserId
                        ? "Bạn"
                        : MaskBidder(item.Bidder?.Username ?? item.Bidder?.FullName),
                    Amount = item.Amount!.Value,
                    BidTime = item.BidTime!.Value,
                    IsCurrentUser = item.BidderId == currentUserId
                })
                .ToList()
        };
    }

    private async Task<int?> FindAuctionOrderIdAsync(
        Product product,
        int winnerId,
        CancellationToken cancellationToken)
    {
        if (!product.AuctionEndTime.HasValue)
        {
            return null;
        }

        return await context.OrderItems
            .AsNoTracking()
            .Where(item =>
                item.ProductId == product.Id &&
                item.OrderId.HasValue &&
                item.Order != null &&
                item.Order.BuyerId == winnerId &&
                item.Order.OrderDate >= product.AuctionEndTime.Value)
            .OrderBy(item => item.OrderId)
            .Select(item => item.OrderId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static decimal BidIncrement(decimal currentPrice) => currentPrice >= 1_000_000m ? 100_000m : 1_000m;

    private async Task BroadcastUpdatedAsync(AuctionDetailDto detail, CancellationToken cancellationToken)
    {
        try
        {
            await auctionHub.Clients.Group(AuctionHub.GroupName(detail.ProductId)).SendAsync(
                "AuctionUpdated",
                new AuctionRealtimeDto
                {
                    ProductId = detail.ProductId,
                    CurrentPrice = detail.CurrentPrice,
                    MinimumNextBid = detail.MinimumNextBid,
                    BidCount = detail.BidCount,
                    EndTime = detail.EndTime,
                    Status = detail.Status
                },
                cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Không thể broadcast cập nhật đấu giá cho product {ProductId}.", detail.ProductId);
        }
    }

    private async Task BroadcastEndedAsync(
        Product product,
        string type,
        string message,
        int? orderId,
        CancellationToken cancellationToken)
    {
        try
        {
            await auctionHub.Clients.Group(AuctionHub.GroupName(product.Id)).SendAsync(
                "AuctionEnded",
                new AuctionNotificationDto
                {
                    ProductId = product.Id,
                    Type = type,
                    Message = message,
                    OrderId = orderId
                },
                cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Không thể broadcast kết thúc đấu giá cho product {ProductId}.", product.Id);
        }
    }

    private async Task SendUserNotificationAsync(
        int userId,
        AuctionNotificationDto notification,
        CancellationToken cancellationToken)
    {
        try
        {
            await notificationHub.Clients.Group($"User_{userId}")
                .SendAsync("ReceiveAuctionNotification", notification, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                exception,
                "Không thể gửi thông báo đấu giá {NotificationType} cho user {UserId}.",
                notification.Type,
                userId);
        }
    }

    private static string MaskBidder(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "Người mua ẩn danh";
        var trimmed = name.Trim();
        return trimmed.Length == 1 ? $"{trimmed[0]}***" : $"{trimmed[0]}***{trimmed[^1]}";
    }

    private static decimal CalculateShippingFee(Address address) =>
        IsHoChiMinhCity(address.City) ? 30_000m : 50_000m;

    private static bool IsHoChiMinhCity(string? city)
    {
        if (string.IsNullOrWhiteSpace(city)) return false;
        var normalized = city.Trim().ToLowerInvariant();
        return normalized.Contains("hồ chí minh") || normalized.Contains("ho chi minh") || normalized.Contains("hcm");
    }
}
