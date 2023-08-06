using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmallMealPlan.Data;
using SmallMealPlan.SmallLister;
using SmallMealPlan.SmallLister.Webhook;

namespace SmallMealPlan.Web.Controllers;

[ApiController]
[AllowAnonymous]
public class SmallListerWebhookApiController : ControllerBase
{
    private readonly ILogger<SmallListerWebhookApiController> _logger;
    private readonly IUserAccountRepository _userAccountRepository;
    private readonly ISmallListerClient _smlClient;
    private readonly IShoppingListRepository _shoppingListRepository;

    public SmallListerWebhookApiController(
        ILogger<SmallListerWebhookApiController> logger,
        IUserAccountRepository userAccountRepository,
        ISmallListerClient smlClient,
        IShoppingListRepository shoppingListRepository)
    {
        _logger = logger;
        _userAccountRepository = userAccountRepository;
        _smlClient = smlClient;
        _shoppingListRepository = shoppingListRepository;
    }

    [HttpPost("~/api/webhook/{userid}/smalllister/list")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> HandleListChangeAsync(int userid, IEnumerable<ListChange> request)
    {
        _logger.LogInformation($"Received list change webhook for userid: {userid}: [{string.Join(',', request.Select(lc => $"(listid={lc.ListId} event={lc.Event})"))}]");
        var user = await _userAccountRepository.GetUserAccountAsync(userid);
        if (user == null)
        {
            _logger.LogWarning($"No user account found with id {userid}");
            return BadRequest();
        }

        foreach (var listChange in request.Where(itemChange => itemChange.ListId == user.SmallListerSyncListId))
        {
            if (string.Equals(listChange.Event, "Modify", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation($"Sync list {user.SmallListerSyncListId} has been changed - updating list name (if applicable)");
                var smlList = await _smlClient.GetListAsync(user.SmallListerToken, user.SmallListerSyncListId);
                if (smlList == null)
                {
                    _logger.LogWarning($"Could not get list from small:lister {user.SmallListerSyncListId}");
                    // TODO: anything here?
                    continue;
                }

                user.SmallListerSyncListName = smlList.Name;
                await _userAccountRepository.UpdateAsync(user);
            }
            else if (string.Equals(listChange.Event, "Delete", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation($"Sync list {user.SmallListerSyncListId} has been deleted - removing small:lister sync");
                user.SmallListerToken = null;
                user.SmallListerLastListId = null;
                await _userAccountRepository.UpdateAsync(user);
            }
        }
        
        return Ok();
    }

    [HttpPost("~/api/webhook/{userid}/smalllister/listitem")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> HandleListItemChangeAsync(int userid, IEnumerable<ListItemChange> request)
    {
        _logger.LogInformation($"Received list item change webhook for userid: {userid}: [{string.Join(',', request.Select(lc => $"(listid={lc.ListId} listitemid={lc.ListItemId} event={lc.Event})"))}]");
        var user = await _userAccountRepository.GetUserAccountAsync(userid);
        if (user == null)
        {
            _logger.LogWarning($"No user account found with id {userid}");
            return BadRequest();
        }

        if (request.Any(itemChange => itemChange.ListId == user.SmallListerSyncListId))
        {
            var smlList = await _smlClient.GetListAsync(user.SmallListerToken, user.SmallListerSyncListId);
            if (smlList == null)
            {
                _logger.LogWarning($"Could not get list from small:lister {user.SmallListerSyncListId}");
                // TODO: should we do anything here? perhaps we should return a BadRequest / InternalServerError?
                // Maybe need to turn off the sync with this list id...perhaps it's gone (although that should raise a list change event webhook)
                return Ok();
            }

            var currentList = await _shoppingListRepository.GetActiveItemsAsync(user);
            foreach (var itemToAddToShoppingList in smlList.Items.Select(i => i.Description).Except(currentList.Select(i => i.Ingredient.Description), StringComparer.InvariantCultureIgnoreCase))
            {
                _logger.LogTrace($"Adding item [{itemToAddToShoppingList}] to shopping list");
                await _shoppingListRepository.AddNewIngredientAsync(user, itemToAddToShoppingList);
            }

            _logger.LogTrace("Removing items from shopping list not in small:lister list");
            await _shoppingListRepository.MarkAsBoughtAsync(user, currentList.ExceptBy(smlList.Items.Select(i => i.Description), sli => sli.Ingredient.Description, StringComparer.InvariantCultureIgnoreCase));
        }
        else
        {
            _logger.LogInformation($"No changes to items in the sync list ({user.SmallListerSyncListId}): [{string.Join(',', request.Select(r => $"{r.ListId}:{r.ListItemId}:{r.Event}"))}]");
        }

        return Ok();
    }
}
