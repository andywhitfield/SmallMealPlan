using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmallMealPlan.SmallLister;

public interface ISmallListerClient
{
    Task<SmallListerList> GetListAsync(string? refreshToken, string listId);
    Task AddItemAsync(string? refreshToken, string listId, string itemToAddToList);
    Task DeleteItemAsync(string? refreshToken, string listId, string itemToAddToList);
    Task<IEnumerable<SmallListerList>> GetListsAsync(string? refreshToken);
    Task RegisterWebhookAsync(string? refreshToken, Uri webhookBaseUri, string userId);
    Task UnregisterWebhookAsync(string? refreshToken);
}