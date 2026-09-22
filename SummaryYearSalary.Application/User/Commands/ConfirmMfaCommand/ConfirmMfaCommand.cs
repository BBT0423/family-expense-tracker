using SummaryYearSalary.Application.Common;

using FluentValidation;

using MediatR;

namespace SummaryYearSalary.Application.User.Commands.ConfirmMfaCommand;

[SensitiveRequest]
public record ConfirmMfaCommand(string UserId, string Code, string IpAddress) : IRequest<Result<bool>>;

public class ConfirmMfaCommandValidator : AbstractValidator<ConfirmMfaCommand>
{
    public ConfirmMfaCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Verification code is required.")
            .Matches(@"^\d{6}$").WithMessage("Verification code must be 6 digits.");
    }
}
