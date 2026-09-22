using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Application.Users.Responses;
using SummaryYearSalary.Domain.Entities;

using MediatR;

using Microsoft.AspNetCore.Identity;

namespace SummaryYearSalary.Application.Users.Queries.LookupUserByEmailQuery;

/// <summary>Exact e-mail match only, never a list, so it cannot be used to enumerate users.</summary>
public class LookupUserByEmailQueryHandler(
    UserManager<ApplicationUser> userManager
) : IRequestHandler<LookupUserByEmailQuery, Result<UserLookupResponse>>
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;

    public async Task<Result<UserLookupResponse>> Handle(LookupUserByEmailQuery request, CancellationToken cancellationToken)
    {
        var user = string.IsNullOrWhiteSpace(request.Email) ? null : await _userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null)
            return Result.Failure<UserLookupResponse>("ไม่พบผู้ใช้ที่ใช้อีเมลนี้", 404);

        if (user.Id == request.RequesterUserId)
            return Result.Failure<UserLookupResponse>("ไม่สามารถลงรายการให้ตัวเองผ่านช่องทางนี้ได้", 400);

        return Result.Success(new UserLookupResponse(UserDisplayName.Of(user), user.Email ?? request.Email));
    }
}
