using ClonEbay_CoreAPI.Services.Interfaces;

namespace ClonEbay_CoreAPI.Services.Implementations;

public sealed class AuctionFinalizerService(
    IServiceScopeFactory scopeFactory,
    ILogger<AuctionFinalizerService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var auctionService = scope.ServiceProvider.GetRequiredService<IAuctionService>();
                var finalized = await auctionService.FinalizeDueAuctionsAsync(stoppingToken);
                if (finalized > 0)
                {
                    logger.LogInformation("Đã kết thúc {AuctionCount} phiên đấu giá.", finalized);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Không thể xử lý các phiên đấu giá đến hạn.");
            }

            await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
        }
    }
}
