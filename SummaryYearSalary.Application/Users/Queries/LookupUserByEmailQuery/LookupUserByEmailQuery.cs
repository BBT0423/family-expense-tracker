using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Application.Users.Responses;

using MediatR;

namespace SummaryYearSalary.Application.Users.Queries.LookupUserByEmailQuery;

public record LookupUserByEmailQuery(string RequesterUserId, string Email) : IRequest<Result<UserLookupResponse>>;
