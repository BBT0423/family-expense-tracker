using SummaryYearSalary.Api.Models.Budget;
using SummaryYearSalary.Application.Interfaces;
using SummaryYearSalary.Application.Transactions.Commands.BulkImportTransactionsCommand;
using SummaryYearSalary.Application.Transactions.Commands.CreateTransactionCommand;
using SummaryYearSalary.Application.Transactions.Commands.DeleteTransactionCommand;
using SummaryYearSalary.Application.Transactions.Commands.UpdateTransactionCommand;
using SummaryYearSalary.Application.Transactions.Queries.GetTransactionsQuery;
using SummaryYearSalary.Domain.Enums;

using MapsterMapper;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SummaryYearSalary.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class TransactionsController(ISender mediator, IMapper mapper, IAppLogger logger) : BaseController
{
    private readonly ISender _mediator = mediator;
    private readonly IMapper _mapper = mapper;
    private readonly IAppLogger _logger = logger;

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] GetTransactionsRequest request)
    {
        var userId = GetCurrentUserId();

        TransactionType? transactionType = null;
        if (!string.IsNullOrWhiteSpace(request.Type) && Enum.TryParse<TransactionType>(request.Type, true, out var parsed))
            transactionType = parsed;

        var query = new GetTransactionsQuery(
            userId, request.Month ?? 0, request.Year ?? 0,
            transactionType, request.CategoryId,
            request.SearchTerm, request.PageNumber ?? 1, request.PageSize ?? 20,
            request.UserId);

        var result = await _mediator.Send(query);
        return result.IsSuccess ? Ok(result) : ToProblem(result, "Get Transactions Failed");
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateTransactionRequest request)
    {
        var userId = GetCurrentUserId();

        var command = _mapper.Map<CreateTransactionCommand>(request) with { UserId = userId, CreatedByUserId = userId };
        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
        {
            _logger.LogDebug("Create transaction failed for user {UserId}", userId);
            return ToProblem(result, "Create Transaction Failed");
        }
        return Ok(result);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTransactionRequest request)
    {
        var userId = GetCurrentUserId();

        var command = _mapper.Map<UpdateTransactionCommand>(request) with { Id = id, UserId = userId };
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : ToProblem(result, "Update Transaction Failed");
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetCurrentUserId();

        var command = new DeleteTransactionCommand(id, userId);
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : ToProblem(result, "Delete Transaction Failed");
    }

    [HttpPost("bulk-import")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> BulkImport([FromBody] BulkImportRequest request)
    {
        var userId = GetCurrentUserId();

        var items = request.Items.Select(i => _mapper.Map<BulkImportItem>(i));
        var command = new BulkImportTransactionsCommand(userId, items);
        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
        {
            _logger.LogDebug("Bulk import failed for user {UserId}", userId);
            return ToProblem(result, "Bulk Import Failed");
        }
        return Ok(result);
    }
}
