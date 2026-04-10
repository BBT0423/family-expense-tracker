using SummaryYearSalary.Application.Common;

using MediatR;

namespace SummaryYearSalary.Application.Categories.Commands.DeleteCategoryCommand;

public record DeleteCategoryCommand(int Id, string UserId) : IRequest<Result<bool>>;
