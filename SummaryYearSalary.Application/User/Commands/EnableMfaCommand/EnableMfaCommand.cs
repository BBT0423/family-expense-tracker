using SummaryYearSalary.Application.Common;

using FluentValidation;

using MediatR;

namespace SummaryYearSalary.Application.User.Commands.EnableMfaCommand;

public record EnableMfaCommand(string UserId, string IpAddress) : IRequest<Result<EnableMfaResponse>>;

public class EnableMfaCommandValidator : AbstractValidator<EnableMfaCommand>
{
    public EnableMfaCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");
    }
}


