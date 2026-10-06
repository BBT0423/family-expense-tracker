using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Application.Members.Responses;

using FluentValidation;

using MediatR;

namespace SummaryYearSalary.Application.Members.Commands.UpdateMemberCommand;

public record UpdateMemberCommand(
    int Id,
    string Name,
    string Color,
    bool IsActive) : IRequest<Result<MemberResponse>>;

public class UpdateMemberCommandValidator : AbstractValidator<UpdateMemberCommand>
{
    public UpdateMemberCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("ชื่อสมาชิกจำเป็นต้องระบุ")
            .MaximumLength(100).WithMessage("ชื่อสมาชิกต้องไม่เกิน 100 ตัวอักษร");

        RuleFor(x => x.Color)
            .NotEmpty().WithMessage("สีจำเป็นต้องระบุ")
            .Matches(@"^#[0-9A-Fa-f]{6}$").WithMessage("รูปแบบสีไม่ถูกต้อง");
    }
}
