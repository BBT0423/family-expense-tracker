using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Application.Members.Responses;

using MediatR;

namespace SummaryYearSalary.Application.Members.Queries.GetMembersQuery;

public record GetMembersQuery : IRequest<Result<IEnumerable<MemberResponse>>>;
