using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Application.Interfaces.IRepositories;
using SummaryYearSalary.Application.Members.Responses;

using MediatR;

namespace SummaryYearSalary.Application.Members.Queries.GetMembersQuery;

public class GetMembersQueryHandler(
    IFamilyMemberRepository memberRepository
) : IRequestHandler<GetMembersQuery, Result<IEnumerable<FamilyMemberResponse>>>
{
    private readonly IFamilyMemberRepository _memberRepository = memberRepository;

    public async Task<Result<IEnumerable<FamilyMemberResponse>>> Handle(GetMembersQuery request, CancellationToken cancellationToken)
    {
        var members = await _memberRepository.GetByUserIdAsync(request.UserId);
        var response = members.Select(m => new FamilyMemberResponse(
            m.Id, m.Name, m.Color, m.Avatar, m.IsActive, m.CreatedAt));
        return Result.Success(response);
    }
}
