using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmallMealPlan.SmallLister;

namespace SmallMealPlan.Web;

public class SmallListerSendQueueHostedService : BackgroundService
{
    private readonly ILogger<SmallListerSendQueueHostedService> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ISmallListerSendQueue _sendQueue;

    public SmallListerSendQueueHostedService(ILogger<SmallListerSendQueueHostedService> logger,
        IServiceScopeFactory serviceScopeFactory,
        ISmallListerSendQueue sendQueue)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        _sendQueue = sendQueue;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Small:Lister Send Queue Hosted Service starting");
        int attempt = 0;

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogTrace("Getting item from the queue...");
            (bool isAddItem, string refreshToken, string listId, string itemToAddOrRemove) = await _sendQueue.DequeueAsync(stoppingToken);

            if (stoppingToken.IsCancellationRequested)
                break;

            _logger.LogTrace("Got a new item from the queue, calling small:lister");

            try
            {
                await using var scope = _serviceScopeFactory.CreateAsyncScope();
                var smallListerClient = scope.ServiceProvider.GetRequiredService<ISmallListerClient>();

                if (isAddItem)
                {
                    _logger.LogInformation($"Adding item {itemToAddOrRemove} to small:lister list {listId}");
                    await smallListerClient.AddItemAsync(refreshToken, listId, itemToAddOrRemove);
                }
                else
                {
                    _logger.LogInformation($"Removing item {itemToAddOrRemove} from small:lister list {listId}");
                    await smallListerClient.DeleteItemAsync(refreshToken, listId, itemToAddOrRemove);
                }

                attempt = 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred sending request to Small:Lister: item: {itemToAddOrRemove}, add? {isAddItem}");
                if (attempt++ < 3)
                {
                    // TODO: replace this with a polly retry
                    // let's wait a short moment and try again
                    await Task.Delay(TimeSpan.FromSeconds(attempt), stoppingToken);
                    _logger.LogInformation($"Re-queued item {itemToAddOrRemove}");
                    await _sendQueue.QueueItemAsync(isAddItem, refreshToken, listId, itemToAddOrRemove);
                }
            }
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Small:Lister Send Queue Hosted Service is stopping.");
        await base.StopAsync(stoppingToken);
    }
}