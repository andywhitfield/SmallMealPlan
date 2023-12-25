using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmallMealPlan.Data;
using SmallMealPlan.RememberTheMilk;
using SmallMealPlan.Web.Model.ShoppingList;

namespace SmallMealPlan.Web.Controllers;

[ApiController]
[Authorize]
public class RtmApiController(
    IUserAccountRepository userAccountRepository,
    IRtmClient rtmClient)
    : ControllerBase
{
    [HttpGet("~/api/rtm")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<GetListsResponse>> GetLists()
    {
        var user = await userAccountRepository.GetUserAccountAsync(User);
        if (string.IsNullOrEmpty(user.RememberTheMilkToken))
            return BadRequest();

        var rtmList = await rtmClient.GetListsAsync(user.RememberTheMilkToken);
        if (rtmList.List == null)
            return new GetListsResponse();

        return new GetListsResponse { Options = rtmList.List.Select(l => new GetListsResponse.ListItem(l.Id ?? "", l.Name ?? "", !string.IsNullOrEmpty(user.RememberTheMilkLastListId) && user.RememberTheMilkLastListId == l.Id)).ToArray() };
    }
}