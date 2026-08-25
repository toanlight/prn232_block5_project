using Microsoft.AspNetCore.SignalR;

namespace ClonEbay_CoreAPI.Hubs;

public sealed class AuctionHub : Hub
{
    public Task JoinAuction(int productId)
    {
        if (productId <= 0)
        {
            throw new HubException("Sản phẩm đấu giá không hợp lệ.");
        }

        return Groups.AddToGroupAsync(Context.ConnectionId, GroupName(productId));
    }

    public Task LeaveAuction(int productId) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(productId));

    public static string GroupName(int productId) => $"Auction_{productId}";
}
