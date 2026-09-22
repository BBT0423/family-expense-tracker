using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Domain.Enums;

using FluentValidation;

using MediatR;

namespace SummaryYearSalary.Application.TransactionProposals.Commands.CreateTransactionProposalCommand;

public record CreateTransactionProposalCommand(
    string ProposedByUserId,
    string RecipientEmail,
    TransactionType Type,
    decimal Amount,
    int CategoryId,
    string? Note,
    DateTime TransactionDate) : IRequest<Result<int>>;

public class CreateTransactionProposalCommandValidator : AbstractValidator<CreateTransactionProposalCommand>
{
    public CreateTransactionProposalCommandValidator()
    {
        RuleFor(x => x.RecipientEmail)
            .NotEmpty().WithMessage("กรุณาระบุอีเมลของผู้รับ")
            .EmailAddress().WithMessage("รูปแบบอีเมลไม่ถูกต้อง");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("กรุณาเลือกหมวดหมู่");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("จำนวนเงินต้องมากกว่า 0");

        RuleFor(x => x.TransactionDate)
            .NotEmpty().WithMessage("วันที่จำเป็นต้องระบุ");

        RuleFor(x => x.Note)
            .MaximumLength(500).WithMessage("หมายเหตุต้องไม่เกิน 500 ตัวอักษร")
            .When(x => x.Note is not null);
    }
}
