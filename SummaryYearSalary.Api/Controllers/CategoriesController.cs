using SummaryYearSalary.Api.Models.Budget;
using SummaryYearSalary.Application.Categories.Commands.CreateCategoryCommand;
using SummaryYearSalary.Application.Categories.Commands.DeleteCategoryCommand;
using SummaryYearSalary.Application.Categories.Commands.UpdateCategoryCommand;
using SummaryYearSalary.Application.Categories.Queries.GetCategoriesQuery;
using SummaryYearSalary.Application.Interfaces;
using SummaryYearSalary.Domain.Enums;

using MapsterMapper;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SummaryYearSalary.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class CategoriesController(ISender mediator, IMapper mapper, IAppLogger logger) : BaseController
{
    private readonly ISender _mediator = mediator;
    private readonly IMapper _mapper = mapper;
    private readonly IAppLogger _logger = logger;

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] string? type = null)
    {
        TransactionType? transactionType = null;
        if (!string.IsNullOrWhiteSpace(type) && Enum.TryParse<TransactionType>(type, true, out var parsed))
            transactionType = parsed;

        var query = new GetCategoriesQuery(GetCurrentUserId(), transactionType);
        var result = await _mediator.Send(query);
        return result.IsSuccess ? Ok(result) : ToProblem(result, "Get Categories Failed");
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request)
    {
        var command = _mapper.Map<CreateCategoryCommand>(request) with { UserId = GetCurrentUserId() };
        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
        {
            _logger.LogDebug("Create category failed for user {UserId}", GetCurrentUserId());
            return ToProblem(result, "Create Category Failed");
        }
        return Ok(result);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryRequest request)
    {
        var command = _mapper.Map<UpdateCategoryCommand>(request) with { Id = id, UserId = GetCurrentUserId() };
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : ToProblem(result, "Update Category Failed");
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id)
    {
        var command = new DeleteCategoryCommand(id, GetCurrentUserId());
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : ToProblem(result, "Delete Category Failed");
    }
}
