using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Application.Users.Responses;

using MediatR;

namespace SummaryYearSalary.Application.Users.Queries.GetUserDirectoryQuery;

public record GetUserDirectoryQuery(string RequesterUserId) : IRequest<Result<IEnumerable<UserDirectoryEntry>>>;
