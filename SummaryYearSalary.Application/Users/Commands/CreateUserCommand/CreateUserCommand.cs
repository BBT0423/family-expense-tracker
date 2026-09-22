using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Application.Users.Responses;

using FluentValidation;

using MediatR;

namespace SummaryYearSalary.Application.Users.Commands.CreateUserCommand;

public record CreateUserCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string Role) : IRequest<Result<UserResponse>>;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches(@"\d").WithMessage("Password must contain at least one digit.")
            .Matches(@"[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .MaximumLength(100);

        RuleFor(x => x.Role)
            .Must(r => r is "Admin" or "User").WithMessage("Role must be either 'Admin' or 'User'.");
    }
}
