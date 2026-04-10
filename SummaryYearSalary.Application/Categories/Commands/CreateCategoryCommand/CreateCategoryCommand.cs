using SummaryYearSalary.Application.Categories.Responses;
using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Domain.Enums;

using FluentValidation;

using MediatR;

namespace SummaryYearSalary.Application.Categories.Commands.CreateCategoryCommand;

public record CreateCategoryCommand(
    string UserId,
    string Name,
    TransactionType Type,
    string Icon,
    string Color) : IRequest<Result<CategoryResponse>>;

public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("ชื่อหมวดหมู่จำเป็นต้องระบุ")
            .MaximumLength(100).WithMessage("ชื่อหมวดหมู่ต้องไม่เกิน 100 ตัวอักษร");

        RuleFor(x => x.Icon)
            .NotEmpty().WithMessage("ไอคอนจำเป็นต้องระบุ");

        RuleFor(x => x.Color)
            .NotEmpty().WithMessage("สีจำเป็นต้องระบุ")
            .Matches(@"^#[0-9A-Fa-f]{6}$").WithMessage("รูปแบบสีไม่ถูกต้อง");
    }
}
