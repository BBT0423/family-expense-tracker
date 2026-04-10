using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Application.Users.Responses;

using MediatR;

namespace SummaryYearSalary.Application.Users.Queries.GetUsersQuery;

public record GetUsersQuery : IRequest<Result<IEnumerable<UserResponse>>>;
