using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Application.Interfaces;
using SummaryYearSalary.Application.Interfaces.IRepositories;
using SummaryYearSalary.Application.Members.Responses;
using SummaryYearSalary.Domain.Entities;

using MediatR;

namespace SummaryYearSalary.Application.Members.Commands.CreateMemberCommand;

public class CreateMemberCommandHandler(
    IMemberRepository memberRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<CreateMemberCommand, Result<MemberResponse>>
{
    private readonly IMemberRepository _memberRepository = memberRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Result<MemberResponse>> Handle(CreateMemberCommand request, CancellationToken cancellationToken)
    {
        var member = new Member
        {
            Name = request.Name,
            Color = request.Color,
            IsActive = true
        };

        await _memberRepository.AddAsync(member);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new MemberResponse(member.Id, member.Name, member.Color, member.IsActive));
    }
}
