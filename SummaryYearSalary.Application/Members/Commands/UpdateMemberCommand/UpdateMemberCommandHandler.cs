using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Application.Interfaces;
using SummaryYearSalary.Application.Interfaces.IRepositories;
using SummaryYearSalary.Application.Members.Responses;

using MediatR;

namespace SummaryYearSalary.Application.Members.Commands.UpdateMemberCommand;

public class UpdateMemberCommandHandler(
    IFamilyMemberRepository memberRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<UpdateMemberCommand, Result<FamilyMemberResponse>>
{
    private readonly IFamilyMemberRepository _memberRepository = memberRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Result<FamilyMemberResponse>> Handle(UpdateMemberCommand request, CancellationToken cancellationToken)
    {
        var member = await _memberRepository.GetByIdAsync(request.Id);
        if (member is null || member.UserId != request.UserId)
            return Result.Failure<FamilyMemberResponse>("ไม่พบสมาชิกที่ต้องการแก้ไข", 404);

        var isDuplicate = await _memberRepository.ExistsByUserIdAndNameAsync(request.UserId, request.Name, request.Id);
        if (isDuplicate)
            return Result.Failure<FamilyMemberResponse>("ชื่อสมาชิกนี้มีอยู่แล้ว");

        member.Name = request.Name;
        member.Color = request.Color;
        member.Avatar = request.Avatar;
        member.IsActive = request.IsActive;

        await _memberRepository.UpdateAsync(member);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new FamilyMemberResponse(
            member.Id, member.Name, member.Color, member.Avatar, member.IsActive, member.CreatedAt));
    }
}
