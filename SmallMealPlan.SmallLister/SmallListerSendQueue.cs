using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace SmallMealPlan.SmallLister;

public class SmallListerSendQueue : ISmallListerSendQueue
{
    private readonly Channel<(bool IsAddItem, string RefreshToken, string ListId, string ItemToAddOrRemove)> _queue =
        Channel.CreateBounded<(bool IsAddItem, string RefreshToken, string ListId, string ItemToAddOrRemove)>(new BoundedChannelOptions(10) { FullMode = BoundedChannelFullMode.Wait });
    
    public ValueTask QueueItemAsync(bool isAdd, string? refreshToken, string listId, string itemToAddToList) =>
        _queue.Writer.WriteAsync((isAdd, refreshToken ?? throw new ArgumentNullException(nameof(refreshToken)), listId, itemToAddToList));

    public ValueTask<(bool IsAddItem, string RefreshToken, string ListId, string ItemToAddOrRemove)> DequeueAsync(CancellationToken cancellationToken) =>
        _queue.Reader.ReadAsync(cancellationToken);
}