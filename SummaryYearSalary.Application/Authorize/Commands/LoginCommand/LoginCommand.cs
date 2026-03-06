using SummaryYearSalary.Application.Common;

using FluentValidation;

using MediatR;

namespace SummaryYearSalary.Application.Authorize.Commands.LoginCommand;

public record LoginCommand(string Email, string Password, string IpAddress) : IRequest<Result<AuthorizeResponse>>;


public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}