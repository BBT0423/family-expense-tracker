using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Application.Interfaces;
using SummaryYearSalary.Application.Interfaces.IRepositories;
using SummaryYearSalary.Application.Members.Responses;
using SummaryYearSalary.Domain.Entities;

using MediatR;

namespace SummaryYearSalary.Application.Members.Commands.CreateMemberCommand;

public class CreateMemberCommandHandler(
    IFamilyMemberRepository memberRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<CreateMemberCommand, Result<FamilyMemberResponse>>
{
    private readonly IFamilyMemberRepository _memberRepository = memberRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Result<FamilyMemberResponse>> Handle(CreateMemberCommand request, CancellationToken cancellationToken)
    {
        var isDuplicate = await _memberRepository.ExistsByUserIdAndNameAsync(request.UserId, request.Name);
        if (isDuplicate)
            return Result.Failure<FamilyMemberResponse>("ชื่อสมาชิกนี้มีอยู่แล้ว");

        var member = new FamilyMember
        {
            UserId = request.UserId,
            Name = request.Name,
            Color = request.Color,
            Avatar = request.Avatar,
            IsActive = true
        };

        await _memberRepository.AddAsync(member);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new FamilyMemberResponse(
            member.Id, member.Name, member.Color, member.Avatar, member.IsActive, member.CreatedAt));
    }
}
