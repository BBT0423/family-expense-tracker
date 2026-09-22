using SummaryYearSalary.Application.Notifications.Commands.MarkAllNotificationsReadCommand;
using SummaryYearSalary.Application.Notifications.Commands.MarkNotificationReadCommand;
using SummaryYearSalary.Application.Notifications.Queries.GetNotificationsQuery;
using SummaryYearSalary.Application.Notifications.Queries.GetUnreadNotificationCountQuery;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SummaryYearSalary.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class NotificationsController(ISender mediator) : BaseController
{
    private readonly ISender _mediator = mediator;

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] bool unreadOnly = false)
    {
        var userId = GetCurrentUserId();

        var result = await _mediator.Send(new GetNotificationsQuery(userId, unreadOnly));
        return result.IsSuccess ? Ok(result) : ToProblem(result, "Get Notifications Failed");
    }

    [HttpGet("unread-count")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userId = GetCurrentUserId();

        var result = await _mediator.Send(new GetUnreadNotificationCountQuery(userId));
        return result.IsSuccess ? Ok(result) : ToProblem(result, "Get Unread Notification Count Failed");
    }

    [HttpPost("{id:int}/read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkRead(int id)
    {
        var userId = GetCurrentUserId();

        var result = await _mediator.Send(new MarkNotificationReadCommand(id, userId));
        return result.IsSuccess ? Ok(result) : ToProblem(result, "Mark Notification Read Failed");
    }

    [HttpPost("read-all")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkAllRead()
    {
        var userId = GetCurrentUserId();

        var result = await _mediator.Send(new MarkAllNotificationsReadCommand(userId));
        return result.IsSuccess ? Ok(result) : ToProblem(result, "Mark All Notifications Read Failed");
    }
}
