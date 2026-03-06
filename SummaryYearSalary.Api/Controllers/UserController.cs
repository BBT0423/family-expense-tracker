using SummaryYearSalary.Api.Helpers;
using SummaryYearSalary.Api.Models.User;
using SummaryYearSalary.Application.Interfaces;
using SummaryYearSalary.Application.User.Commands.ChangePasswordCommand;
using SummaryYearSalary.Application.User.Commands.EnableMfaCommand;

using MapsterMapper;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SummaryYearSalary.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class UserController(ISender mediator, IMapper mapper, IAppLogger logger) : BaseController
{
    private readonly ISender _mediator = mediator;
    private readonly IMapper _mapper = mapper;
    private readonly IAppLogger _logger = logger;

    /// <summary>
    /// Enable Multi-Factor Authentication for the current user.
    /// Addresses OWASP #2 - Broken Authentication.
    /// </summary>
    [HttpPost("mfa/enable")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> EnableMfa()
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var userId = GetCurrentUserId();
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? ApiConstants.Unknown;
        var result = await _mediator.Send(new EnableMfaCommand(userId, ipAddress));

        if (!result.IsSuccess)
        {
            _logger.LogDebug("Enable MFA failed for user {UserId} from {IpAddress}", userId, ipAddress);
            return ToProblem(result, "Enable MFA Failed");
        }

        return Ok(result);
    }


    /// <summary>
    /// Change password for the current user.
    /// Implements secure password handling and notifies user via email (OWASP #5).
    /// </summary>
    /// <param name="request">Change password request containing current and new passwords.</param>
    [HttpPost("change-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var userId = GetCurrentUserId();

        if (userId != request.UserId)
        {
            _logger.LogDebug("Change password attempt with missing user ID from {IpAddress}", HttpContext.Connection.RemoteIpAddress?.ToString() ?? ApiConstants.Unknown);
            return BadRequest("User identity is required.");
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? ApiConstants.Unknown;
        var template = await System.IO.File.ReadAllTextAsync(TemplateConstants.PasswordUpdateTemplate);
        var command = _mapper.Map<ChangePasswordCommand>(request) with { UserId = userId, IpAddress = ipAddress, Template = template };
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            _logger.LogDebug("Change password failed for user {UserId} from {IpAddress}", userId, ipAddress);
            return ToProblem(result, "Change Password Failed");
        }

        return Ok(result);
    }

}