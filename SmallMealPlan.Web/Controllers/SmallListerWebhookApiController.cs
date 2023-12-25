using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmallMealPlan.Data;
using SmallMealPlan.Model;
using SmallMealPlan.SmallLister;
using SmallMealPlan.SmallLister.Webhook;

namespace SmallMealPlan.Web.Controllers;

[ApiController]
[AllowAnonymous]
public class SmallListerWebhookApiController(
    ILogger<SmallListerWebhookApiController> logger,
    IUserAccountRepository userAccountRepository,
    ISmallListerClient smlClient,
    IShoppingListRepository shoppingListRepository)
    : ControllerBase
{
    [HttpPost("~/api/webhook/{userid}/smalllister/list")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> HandleListChangeAsync(int userid, IEnumerable<ListChange> request)
    {
        logger.LogInformation($"Received list change webhook for userid: {userid}: [{string.Join(',', request.Select(lc => $"(listid={lc.ListId} event={lc.Event})"))}]");
        var user = await userAccountRepository.GetUserAccountAsync(userid);
        if (user == null)
        {
            logger.LogWarning($"No user account found with id {userid}");
            return BadRequest();
        }

        foreach (var listChange in request.Where(itemChange => itemChange.ListId == user.SmallListerSyncListId))
        {
            if (string.Equals(listChange.Event, "Modify", StringComparison.OrdinalIgnoreCase))
            {
                logger.LogInformation($"Sync list {user.SmallListerSyncListId} has been changed - updating list name (if applicable)");
                var smlList = await smlClient.GetListAsync(user.SmallListerToken, user.SmallListerSyncListId ?? "");
                if (smlList == null)
                {
                    logger.LogWarning($"Could not get list from small:lister {user.SmallListerSyncListId}");
                    // TODO: anything here?
                    continue;
                }

                user.SmallListerSyncListName = smlList.Name;
                await userAccountRepository.UpdateAsync(user);
            }
            else if (string.Equals(listChange.Event, "Delete", StringComparison.OrdinalIgnoreCase))
            {
                logger.LogInformation($"Sync list {user.SmallListerSyncListId} has been deleted - removing small:lister sync");
                user.SmallListerToken =
                    user.SmallListerLastListId =
                    user.SmallListerSyncListId =
                    user.SmallListerSyncListName = null;
                await userAccountRepository.UpdateAsync(user);
            }
        }
        
        return Ok();
    }

    [HttpPost("~/api/webhook/{userid}/smalllister/listitem")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> HandleListItemChangeAsync(int userid, IEnumerable<ListItemChange> request)
    {
        logger.LogInformation($"Received list item change webhook for userid: {userid}: [{string.Join(',', request.Select(lc => $"(listid={lc.ListId} listitemid={lc.ListItemId} event={lc.Event})"))}]");
        var user = await userAccountRepository.GetUserAccountAsync(userid);
        if (user == null)
        {
            logger.LogWarning($"No user account found with id {userid}");
            return BadRequest();
        }

        if (request.Any(itemChange => itemChange.ListId == user.SmallListerSyncListId))
            await SyncWithSmallListerAsync(user);
        else
            logger.LogInformation($"No changes to items in the sync list ({user.SmallListerSyncListId}): [{string.Join(',', request.Select(r => $"{r.ListId}:{r.ListItemId}:{r.Event}"))}]");

        return Ok();
    }

    private async Task SyncWithSmallListerAsync(UserAccount user)
    {
        var smlList = await smlClient.GetListAsync(user.SmallListerToken, user.SmallListerSyncListId ?? "");
        if (smlList == null)
        {
            logger.LogWarning($"Could not get list from small:lister {user.SmallListerSyncListId}");
            // TODO: should we do anything here? perhaps we should return a BadRequest / InternalServerError?
            // Maybe need to turn off the sync with this list id...perhaps it's gone (although that should raise a list change event webhook)
            return;
        }

        var currentList = await shoppingListRepository.GetActiveItemsAsync(user);
        foreach (var itemToAddToShoppingList in (smlList.Items ?? Enumerable.Empty<SmallListerItem>()).Select(i => i.Description?.Trim() ?? "").Except(currentList.Select(i => i.Ingredient.Description.Trim()), StringComparer.InvariantCultureIgnoreCase))
        {
            logger.LogTrace($"Adding item [{itemToAddToShoppingList}] to shopping list");
            await shoppingListRepository.AddNewIngredientAsync(user, itemToAddToShoppingList);
        }

        logger.LogTrace("Removing items from shopping list not in small:lister list");
        await shoppingListRepository.MarkAsBoughtAsync(user, currentList.ExceptBy((smlList.Items ?? Enumerable.Empty<SmallListerItem>()).Select(i => i.Description?.Trim() ?? ""), sli => sli.Ingredient.Description.Trim(), StringComparer.InvariantCultureIgnoreCase));
    }
}
