using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Application.Interfaces;
using SummaryYearSalary.Application.Interfaces.IRepositories;

using MediatR;

namespace SummaryYearSalary.Application.Members.Commands.DeleteMemberCommand;

public class DeleteMemberCommandHandler(
    IFamilyMemberRepository memberRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<DeleteMemberCommand, Result<bool>>
{
    private readonly IFamilyMemberRepository _memberRepository = memberRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Result<bool>> Handle(DeleteMemberCommand request, CancellationToken cancellationToken)
    {
        var member = await _memberRepository.GetByIdAsync(request.Id);
        if (member is null || member.UserId != request.UserId)
            return Result.Failure<bool>("ไม่พบสมาชิกที่ต้องการลบ", 404);

        member.IsDeleted = true;
        await _memberRepository.UpdateAsync(member);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(true);
    }
}
