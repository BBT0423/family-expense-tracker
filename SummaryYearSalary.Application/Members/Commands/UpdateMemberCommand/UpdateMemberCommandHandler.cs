using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Application.Interfaces;
using SummaryYearSalary.Application.Interfaces.IRepositories;
using SummaryYearSalary.Application.Members.Responses;

using MediatR;

namespace SummaryYearSalary.Application.Members.Commands.UpdateMemberCommand;

public class UpdateMemberCommandHandler(
    IMemberRepository memberRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<UpdateMemberCommand, Result<MemberResponse>>
{
    private readonly IMemberRepository _memberRepository = memberRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Result<MemberResponse>> Handle(UpdateMemberCommand request, CancellationToken cancellationToken)
    {
        var member = await _memberRepository.GetByIdAsync(request.Id);
        if (member is null)
            return Result.Failure<MemberResponse>("ไม่พบสมาชิกที่ต้องการแก้ไข", 404);

        member.Name = request.Name;
        member.Color = request.Color;
        member.IsActive = request.IsActive;

        await _memberRepository.UpdateAsync(member);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new MemberResponse(member.Id, member.Name, member.Color, member.IsActive));
    }
}
