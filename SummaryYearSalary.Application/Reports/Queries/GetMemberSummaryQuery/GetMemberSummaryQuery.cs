using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Application.Reports.Responses;

using MediatR;

namespace SummaryYearSalary.Application.Reports.Queries.GetMemberSummaryQuery;

public record GetMemberSummaryQuery(string UserId, int Month, int Year) : IRequest<Result<MemberSummaryResponse>>;
