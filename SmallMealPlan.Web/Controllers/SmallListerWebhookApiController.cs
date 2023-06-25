using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmallMealPlan.SmallLister.Webhook;

namespace SmallMealPlan.Web.Controllers;

[ApiController]
[AllowAnonymous]
public class SmallListerWebhookApiController : ControllerBase
{
    private readonly ILogger<SmallListerWebhookApiController> _logger;

    public SmallListerWebhookApiController(ILogger<SmallListerWebhookApiController> logger) => _logger = logger;

    [HttpPost("~/api/webhook/{userid}/smalllister/list")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult HandleListChange(int userid, IEnumerable<ListChange> request)
    {
        _logger.LogInformation($"Received list change webhook for userid: {userid}: [{string.Join(',', request.Select(lc => $"listid: {lc.ListId}; event: {lc.Event}"))}]");
        return Ok();
    }

    [HttpPost("~/api/webhook/{userid}/smalllister/listitem")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult HandleListItemChange(int userid, IEnumerable<ListItemChange> request)
    {
        _logger.LogInformation($"Received list item change webhook for userid: {userid}: [{string.Join(',', request.Select(lc => $"listid: {lc.ListId}; listitemid: {lc.ListItemId}; event: {lc.Event}"))}]");
        return Ok();
    }
}
