using SummaryYearSalary.Api.Models.Budget;
using SummaryYearSalary.Application.TransactionProposals.Commands.AcceptTransactionProposalCommand;
using SummaryYearSalary.Application.TransactionProposals.Commands.CreateTransactionProposalCommand;
using SummaryYearSalary.Application.TransactionProposals.Commands.RejectTransactionProposalCommand;
using SummaryYearSalary.Application.TransactionProposals.Queries.GetIncomingProposalsQuery;
using SummaryYearSalary.Domain.Enums;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SummaryYearSalary.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class TransactionProposalsController(ISender mediator) : BaseController
{
    private readonly ISender _mediator = mediator;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CreateTransactionProposalRequest request)
    {
        if (!Enum.TryParse<TransactionType>(request.Type, true, out var type))
            return BadRequest(new { error = "ประเภทรายการไม่ถูกต้อง" });

        var command = new CreateTransactionProposalCommand(
            GetCurrentUserId(), request.RecipientEmail, type, request.Amount,
            request.CategoryId, request.Note, request.TransactionDate);
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : ToProblem(result, "Create Proposal Failed");
    }

    [HttpGet("incoming")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetIncoming()
    {
        var result = await _mediator.Send(new GetIncomingProposalsQuery(GetCurrentUserId()));
        return result.IsSuccess ? Ok(result) : ToProblem(result, "Get Proposals Failed");
    }

    [HttpPost("{id:int}/accept")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Accept(int id, [FromBody] AcceptTransactionProposalRequest request)
    {
        var command = new AcceptTransactionProposalCommand(GetCurrentUserId(), id, request.CategoryId);
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : ToProblem(result, "Accept Proposal Failed");
    }

    [HttpPost("{id:int}/reject")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Reject(int id)
    {
        var result = await _mediator.Send(new RejectTransactionProposalCommand(GetCurrentUserId(), id));
        return result.IsSuccess ? Ok(result) : ToProblem(result, "Reject Proposal Failed");
    }
}
