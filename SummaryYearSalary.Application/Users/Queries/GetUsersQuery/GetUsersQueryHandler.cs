using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Application.Users.Responses;
using SummaryYearSalary.Domain.Entities;

using MediatR;

using Microsoft.AspNetCore.Identity;

namespace SummaryYearSalary.Application.Users.Queries.GetUsersQuery;

public class GetUsersQueryHandler(
    UserManager<ApplicationUser> userManager
) : IRequestHandler<GetUsersQuery, Result<IEnumerable<UserResponse>>>
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;

    public async Task<Result<IEnumerable<UserResponse>>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var users = _userManager.Users
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .ToList();

        var responses = new List<UserResponse>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            responses.Add(new UserResponse(
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email,
                user.PhoneNumber,
                user.CreatedAt,
                user.LastLoginAt,
                roles));
        }

        return Result.Success<IEnumerable<UserResponse>>(responses);
    }
}
