using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Application.Interfaces.IRepositories;
using SummaryYearSalary.Application.Members.Responses;

using MediatR;

namespace SummaryYearSalary.Application.Members.Queries.GetMembersQuery;

public class GetMembersQueryHandler(
    IMemberRepository memberRepository
) : IRequestHandler<GetMembersQuery, Result<IEnumerable<MemberResponse>>>
{
    private readonly IMemberRepository _memberRepository = memberRepository;

    public async Task<Result<IEnumerable<MemberResponse>>> Handle(GetMembersQuery request, CancellationToken cancellationToken)
    {
        var members = await _memberRepository.GetAllActiveFirstAsync();
        var response = members.Select(m => new MemberResponse(m.Id, m.Name, m.Color, m.IsActive));
        return Result.Success(response);
    }
}
