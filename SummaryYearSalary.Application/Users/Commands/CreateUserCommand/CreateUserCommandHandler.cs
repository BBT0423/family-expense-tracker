using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Application.Users.Responses;
using SummaryYearSalary.Domain.Entities;

using MediatR;

using Microsoft.AspNetCore.Identity;

namespace SummaryYearSalary.Application.Users.Commands.CreateUserCommand;

public class CreateUserCommandHandler(
    UserManager<ApplicationUser> userManager
) : IRequestHandler<CreateUserCommand, Result<UserResponse>>
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;

    public async Task<Result<UserResponse>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var existing = await _userManager.FindByEmailAsync(request.Email);
        if (existing is not null)
            return Result.Failure<UserResponse>("อีเมลนี้มีผู้ใช้งานแล้ว", 409);

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            EmailConfirmed = true
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
            return Result.Failure<UserResponse>($"สร้างผู้ใช้ไม่สำเร็จ: {errors}", 400);
        }

        await _userManager.AddToRoleAsync(user, request.Role);

        return Result.Success(new UserResponse(
            user.Id, user.FirstName, user.LastName, user.Email, user.PhoneNumber,
            user.CreatedAt, user.LastLoginAt, [request.Role]));
    }
}
