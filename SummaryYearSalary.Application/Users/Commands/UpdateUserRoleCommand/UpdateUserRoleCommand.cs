using SummaryYearSalary.Application.Common;

using FluentValidation;

using MediatR;

namespace SummaryYearSalary.Application.Users.Commands.UpdateUserRoleCommand;

public record UpdateUserRoleCommand(string RequesterUserId, string TargetUserId, string Role) : IRequest<Result<bool>>;

public class UpdateUserRoleCommandValidator : AbstractValidator<UpdateUserRoleCommand>
{
    public UpdateUserRoleCommandValidator()
    {
        RuleFor(x => x.Role)
            .Must(r => r is "Admin" or "User").WithMessage("Role must be either 'Admin' or 'User'.");
    }
}
