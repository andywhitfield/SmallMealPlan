using System.Threading;
using System.Threading.Tasks;

namespace SmallMealPlan.SmallLister;

public interface ISmallListerSendQueue
{
    ValueTask QueueItemAsync(bool isAdd, string? refreshToken, string listId, string itemToAddToList);
    ValueTask<(bool IsAddItem, string RefreshToken, string ListId, string ItemToAddOrRemove)> DequeueAsync(CancellationToken cancellationToken);
}