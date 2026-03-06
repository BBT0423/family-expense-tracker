using SummaryYearSalary.Application.Common;

using Microsoft.AspNetCore.Mvc;

namespace SummaryYearSalary.Api.Controllers;

[ApiController]
public abstract class BaseController : ControllerBase
{
    protected ActionResult ToProblem<T>(Result<T> error, string? title)
    {
        ArgumentNullException.ThrowIfNull(error);
        if (error.ValidationErrors.Count != 0)
        {
            var allErrors = error.ValidationErrors.Select(x => x.Value).ToList();
            return Problem(
                detail: string.Join("; ", allErrors.SelectMany(e => e)),
                statusCode: error.StatusCode,
                title: error.Error
            );
        }

        return Problem(
                    statusCode: error.StatusCode,
                    title: title ?? "Validation Error",
                    detail: error.Error
                );
    }

    protected string GetCurrentUserId()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            return userIdClaim?.Value ?? throw new InvalidOperationException("User identity not found in token");
        }
        throw new InvalidOperationException("User identity not found in token");
    }
}
