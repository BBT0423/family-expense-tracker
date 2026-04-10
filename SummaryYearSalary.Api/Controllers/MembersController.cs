using SummaryYearSalary.Api.Models.Budget;
using SummaryYearSalary.Application.Interfaces;
using SummaryYearSalary.Application.Members.Commands.CreateMemberCommand;
using SummaryYearSalary.Application.Members.Commands.DeleteMemberCommand;
using SummaryYearSalary.Application.Members.Commands.UpdateMemberCommand;
using SummaryYearSalary.Application.Members.Queries.GetMembersQuery;

using MapsterMapper;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SummaryYearSalary.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class MembersController(ISender mediator, IMapper mapper, IAppLogger logger) : BaseController
{
    private readonly ISender _mediator = mediator;
    private readonly IMapper _mapper = mapper;
    private readonly IAppLogger _logger = logger;

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var query = new GetMembersQuery(GetCurrentUserId());
        var result = await _mediator.Send(query);
        return result.IsSuccess ? Ok(result) : ToProblem(result, "Get Members Failed");
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateMemberRequest request)
    {
        var command = _mapper.Map<CreateMemberCommand>(request) with { UserId = GetCurrentUserId() };
        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
        {
            _logger.LogDebug("Create member failed for user {UserId}", GetCurrentUserId());
            return ToProblem(result, "Create Member Failed");
        }
        return Ok(result);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateMemberRequest request)
    {
        var command = _mapper.Map<UpdateMemberCommand>(request) with { Id = id, UserId = GetCurrentUserId() };
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : ToProblem(result, "Update Member Failed");
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var command = new DeleteMemberCommand(id, GetCurrentUserId());
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : ToProblem(result, "Delete Member Failed");
    }
}
