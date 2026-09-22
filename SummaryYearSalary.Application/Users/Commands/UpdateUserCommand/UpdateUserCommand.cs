using SummaryYearSalary.Application.Common;

using FluentValidation;

using MediatR;

namespace SummaryYearSalary.Application.Users.Commands.UpdateUserCommand;

public record UpdateUserCommand(string TargetUserId, string FirstName, string LastName) : IRequest<Result<bool>>;

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .MaximumLength(100);
    }
}
