using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Domain.Entities;

using MediatR;

using Microsoft.AspNetCore.Identity;

namespace SummaryYearSalary.Application.Users.Commands.UpdateUserCommand;

public class UpdateUserCommandHandler(
    UserManager<ApplicationUser> userManager
) : IRequestHandler<UpdateUserCommand, Result<bool>>
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;

    public async Task<Result<bool>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.TargetUserId);
        if (user is null)
            return Result.Failure<bool>("ไม่พบผู้ใช้ที่ระบุ", 404);

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        await _userManager.UpdateAsync(user);

        return Result.Success(true);
    }
}
