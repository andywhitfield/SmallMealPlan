using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmallMealPlan.Data;
using SmallMealPlan.SmallLister;
using SmallMealPlan.Web.Model.ShoppingList;

namespace SmallMealPlan.Web.Controllers;

[ApiController]
[Authorize]
public class SmallListerApiController(
    IUserAccountRepository userAccountRepository,
    ISmallListerClient smlClient)
    : ControllerBase
{
    [HttpGet("~/api/sml")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<GetListsResponse>> GetLists()
    {
        var user = await userAccountRepository.GetUserAccountAsync(User);
        if (string.IsNullOrEmpty(user.SmallListerToken))
            return BadRequest();

        var smlLists = await smlClient.GetListsAsync(user.SmallListerToken);
        if (smlLists == null)
            return new GetListsResponse();

        return new GetListsResponse { Options = smlLists.Select(l => new GetListsResponse.ListItem(l.ListId ?? "", l.Name ?? "", !string.IsNullOrEmpty(user.SmallListerLastListId) && user.SmallListerLastListId == l.ListId)).ToArray() };
    }
}