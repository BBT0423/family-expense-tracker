using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Domain.Entities;

using MediatR;

using Microsoft.AspNetCore.Identity;

namespace SummaryYearSalary.Application.Users.Commands.UpdateUserRoleCommand;

public class UpdateUserRoleCommandHandler(
    UserManager<ApplicationUser> userManager
) : IRequestHandler<UpdateUserRoleCommand, Result<bool>>
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;

    public async Task<Result<bool>> Handle(UpdateUserRoleCommand request, CancellationToken cancellationToken)
    {
        var target = await _userManager.FindByIdAsync(request.TargetUserId);
        if (target is null)
            return Result.Failure<bool>("ไม่พบผู้ใช้ที่ระบุ", 404);

        if (request.RequesterUserId == request.TargetUserId && request.Role != "Admin")
            return Result.Failure<bool>("ไม่สามารถถอดสิทธิ์ Admin ของตัวเองได้", 400);

        var currentRoles = await _userManager.GetRolesAsync(target);
        if (currentRoles.Count > 0)
            await _userManager.RemoveFromRolesAsync(target, currentRoles);

        await _userManager.AddToRoleAsync(target, request.Role);

        return Result.Success(true);
    }
}
