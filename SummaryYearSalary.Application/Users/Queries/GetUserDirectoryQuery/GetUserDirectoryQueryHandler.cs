using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Application.Users.Responses;
using SummaryYearSalary.Domain.Entities;

using MediatR;

using Microsoft.AspNetCore.Identity;

namespace SummaryYearSalary.Application.Users.Queries.GetUserDirectoryQuery;

/// <summary>
/// A lightweight, non-Admin-gated user list (name + email only, no phone/role/login history) —
/// used to populate "who to view" / "who to send to" pickers. Excludes the caller so it never
/// duplicates the explicit "self" option those pickers already offer.
/// </summary>
public class GetUserDirectoryQueryHandler(
    UserManager<ApplicationUser> userManager
) : IRequestHandler<GetUserDirectoryQuery, Result<IEnumerable<UserDirectoryEntry>>>
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;

    public Task<Result<IEnumerable<UserDirectoryEntry>>> Handle(GetUserDirectoryQuery request, CancellationToken cancellationToken)
    {
        var entries = _userManager.Users
            .Where(u => u.Id != request.RequesterUserId)
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .AsEnumerable()
            .Select(u => new UserDirectoryEntry(u.Id, UserDisplayName.Of(u), u.Email ?? string.Empty));

        return Task.FromResult(Result.Success(entries));
    }
}
