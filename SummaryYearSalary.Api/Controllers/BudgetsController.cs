using SummaryYearSalary.Api.Models.Budget;
using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Application.Interfaces;
using SummaryYearSalary.Application.Interfaces.IRepositories;
using SummaryYearSalary.Domain.Entities;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SummaryYearSalary.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class BudgetsController(IBudgetRepository budgetRepository, IUnitOfWork unitOfWork) : BaseController
{
    private readonly IBudgetRepository _budgetRepository = budgetRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int? month, [FromQuery] int? year)
    {
        var budgets = await _budgetRepository.GetByUserMonthYearAsync(
            GetCurrentUserId(), month ?? 0, year ?? 0);
        return Ok(budgets.Select(b => new
        {
            b.Id, b.CategoryId, b.Month, b.Year, b.LimitAmount,
            CategoryName = b.Category?.Name,
            CategoryIcon = b.Category?.Icon
        }));
    }

    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Upsert([FromBody] UpsertBudgetRequest request)
    {
        var existing = await _budgetRepository.GetByUserCategoryMonthYearAsync(
            GetCurrentUserId(), request.CategoryId, request.Month, request.Year);

        if (existing is null)
        {
            var budget = new Budget
            {
                UserId = GetCurrentUserId(),
                CategoryId = request.CategoryId,
                Month = request.Month,
                Year = request.Year,
                LimitAmount = request.LimitAmount
            };
            await _budgetRepository.AddAsync(budget);
        }
        else
        {
            existing.LimitAmount = request.LimitAmount;
            await _budgetRepository.UpdateAsync(existing);
        }

        await _unitOfWork.SaveChangesAsync();
        return Ok(new { Message = "Budget saved successfully" });
    }
}
