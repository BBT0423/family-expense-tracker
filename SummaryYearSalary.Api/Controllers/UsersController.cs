using SummaryYearSalary.Application.Users.Queries.GetUsersQuery;

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
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var query = new GetUsersQuery();
        var result = await _mediator.Send(query);
        return result.IsSuccess ? Ok(result) : ToProblem(result, "Get Users Failed");
    }
}
