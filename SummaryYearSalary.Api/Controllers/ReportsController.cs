using SummaryYearSalary.Application.Interfaces;
using SummaryYearSalary.Application.Reports.Queries.ExportMonthlyReportQuery;
using SummaryYearSalary.Application.Reports.Queries.GetCategorySummaryQuery;
using SummaryYearSalary.Application.Reports.Queries.GetMonthlySummaryQuery;
using SummaryYearSalary.Application.Reports.Queries.GetYearlySummaryQuery;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SummaryYearSalary.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ReportsController(ISender mediator, IAppLogger logger) : BaseController
{
    private readonly ISender _mediator = mediator;
    private readonly IAppLogger _logger = logger;

    [HttpGet("summary")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMonthlySummary([FromQuery] int month, [FromQuery] int year, [FromQuery] string? userId = null, [FromQuery] int? memberId = null)
    {
        var query = new GetMonthlySummaryQuery(GetCurrentUserId(), month, year, userId, memberId);
        var result = await _mediator.Send(query);
        return result.IsSuccess ? Ok(result) : ToProblem(result, "Get Summary Failed");
    }

    [HttpGet("yearly")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetYearlySummary([FromQuery] int year, [FromQuery] string? userId = null, [FromQuery] int? memberId = null)
    {
        var query = new GetYearlySummaryQuery(GetCurrentUserId(), year, userId, memberId);
        var result = await _mediator.Send(query);
        return result.IsSuccess ? Ok(result) : ToProblem(result, "Get Yearly Summary Failed");
    }

    [HttpGet("by-category")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategorySummary([FromQuery] int month, [FromQuery] int year, [FromQuery] string? userId = null, [FromQuery] int? memberId = null)
    {
        var query = new GetCategorySummaryQuery(GetCurrentUserId(), month, year, userId, memberId);
        var result = await _mediator.Send(query);
        return result.IsSuccess ? Ok(result) : ToProblem(result, "Get Category Summary Failed");
    }

    [HttpGet("export")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportMonthly([FromQuery] int month, [FromQuery] int year, [FromQuery] string? userId = null)
    {
        var requesterId = GetCurrentUserId();
        var query = new ExportMonthlyReportQuery(requesterId, month, year, userId);
        var result = await _mediator.Send(query);
        if (!result.IsSuccess)
        {
            _logger.LogDebug("Export failed for user {UserId}", requesterId);
            return ToProblem(result, "Export Failed");
        }

        var fileName = $"report-{year}-{month:D2}.xlsx";
        return File(result.Value!, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }
}
