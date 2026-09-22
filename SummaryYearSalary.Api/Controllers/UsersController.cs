using SummaryYearSalary.Api.Models.Users;
using SummaryYearSalary.Application.Users.Commands.CreateUserCommand;
using SummaryYearSalary.Application.Users.Commands.UpdateUserCommand;
using SummaryYearSalary.Application.Users.Commands.UpdateUserRoleCommand;
using SummaryYearSalary.Application.Users.Queries.GetUserDirectoryQuery;
using SummaryYearSalary.Application.Users.Queries.GetUsersQuery;
using SummaryYearSalary.Application.Users.Queries.LookupUserByEmailQuery;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SummaryYearSalary.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class UsersController(ISender mediator) : BaseController
{
    private readonly ISender _mediator = mediator;

    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var query = new GetUsersQuery();
        var result = await _mediator.Send(query);
        return result.IsSuccess ? Ok(result) : ToProblem(result, "Get Users Failed");
    }

    [HttpGet("directory")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDirectory()
    {
        var query = new GetUserDirectoryQuery(GetCurrentUserId());
        var result = await _mediator.Send(query);
        return result.IsSuccess ? Ok(result) : ToProblem(result, "Get User Directory Failed");
    }

    [HttpGet("lookup")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Lookup([FromQuery] string email)
    {
        var query = new LookupUserByEmailQuery(GetCurrentUserId(), email);
        var result = await _mediator.Send(query);
        return result.IsSuccess ? Ok(result) : ToProblem(result, "Lookup User Failed");
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
    {
        var command = new CreateUserCommand(request.FirstName, request.LastName, request.Email, request.Password, request.Role);
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : ToProblem(result, "Create User Failed");
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateUserRequest request)
    {
        var command = new UpdateUserCommand(id, request.FirstName, request.LastName);
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : ToProblem(result, "Update User Failed");
    }

    [HttpPut("{id}/role")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateRole(string id, [FromBody] UpdateUserRoleRequest request)
    {
        var command = new UpdateUserRoleCommand(GetCurrentUserId(), id, request.Role);
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : ToProblem(result, "Update User Role Failed");
    }
}
