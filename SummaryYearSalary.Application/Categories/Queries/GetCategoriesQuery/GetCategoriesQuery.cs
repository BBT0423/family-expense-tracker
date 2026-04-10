using SummaryYearSalary.Application.Categories.Responses;
using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Domain.Enums;

using MediatR;

namespace SummaryYearSalary.Application.Categories.Queries.GetCategoriesQuery;

public record GetCategoriesQuery(string UserId, TransactionType? Type = null) : IRequest<Result<IEnumerable<CategoryResponse>>>;
