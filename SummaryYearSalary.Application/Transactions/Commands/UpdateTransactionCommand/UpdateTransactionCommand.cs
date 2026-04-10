using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Application.Transactions.Responses;
using SummaryYearSalary.Domain.Enums;

using FluentValidation;

using MediatR;

namespace SummaryYearSalary.Application.Transactions.Commands.UpdateTransactionCommand;

public record UpdateTransactionCommand(
    int Id,
    string UserId,
    int MemberId,
    int CategoryId,
    TransactionType Type,
    decimal Amount,
    string? Note,
    DateTime TransactionDate) : IRequest<Result<TransactionResponse>>;

public class UpdateTransactionCommandValidator : AbstractValidator<UpdateTransactionCommand>
{
    public UpdateTransactionCommandValidator()
    {
        RuleFor(x => x.MemberId).GreaterThan(0).WithMessage("กรุณาเลือกสมาชิก");
        RuleFor(x => x.CategoryId).GreaterThan(0).WithMessage("กรุณาเลือกหมวดหมู่");
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("จำนวนเงินต้องมากกว่า 0");
        RuleFor(x => x.TransactionDate).NotEmpty().WithMessage("วันที่จำเป็นต้องระบุ");
        RuleFor(x => x.Note)
            .MaximumLength(500).WithMessage("หมายเหตุต้องไม่เกิน 500 ตัวอักษร")
            .When(x => x.Note is not null);
    }
}
