using SummaryYearSalary.Api.Helpers;
using SummaryYearSalary.Api.Models.Authorize;
using SummaryYearSalary.Application.Authorize.Commands.ForgotPasswordCommand;
using SummaryYearSalary.Application.Authorize.Commands.LoginCommand;
using SummaryYearSalary.Application.Authorize.Commands.RefreshTokenCommand;
using SummaryYearSalary.Application.Authorize.Commands.ResetPasswordCommand;
using SummaryYearSalary.Application.Authorize.Queries.VerifyMfaQuery;
using SummaryYearSalary.Application.Interfaces;

using MapsterMapper;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SummaryYearSalary.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthorizeController(ISender mediator, IMapper mapper, IAppLogger logger) : BaseController
{
    private readonly ISender _mediator = mediator;
    private readonly IMapper _mapper = mapper;
    private readonly IAppLogger _logger = logger;


    /// <summary>
    /// Authenticate user and return JWT tokens.
    /// Implements MFA support and account lockout (OWASP #2).
    /// </summary>
    /// <param name="request">Login request containing email and password.</param>
    /// <returns>JWT tokens if MFA disabled, or MFA challenge if enabled.</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? ApiConstants.Unknown;
        var command = _mapper.Map<LoginCommand>(request);
        command = command with { IpAddress = ipAddress };
        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
        {
            _logger.LogDebug("Login failed for {Email} from {IpAddress}", request.Email, ipAddress);
            return ToProblem(result, "Login Failed");
        }

        _logger.LogDebug("User {Email} logged in successfully from {IpAddress}. MFA enabled: {MfaEnabled}", 
            request.Email, ipAddress, result.Value!.IsMfaEnabled);

        return Ok(result);
    }


    /// <summary>
    /// Refresh access token using refresh token.
    /// Implements secure token rotation (OWASP #2).
    /// </summary>
    /// <param name="request">Refresh token request containing the refresh token.</param>
    /// <returns>New JWT access and refresh tokens upon successful validation.</returns>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? ApiConstants.Unknown;
        var command = _mapper.Map<RefreshTokenCommand>(request);
        command = command with { IpAddress = ipAddress };
        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
        {
            _logger.LogDebug("Refresh token failed from {IpAddress}", ipAddress);
            return ToProblem(result, "Refresh Token Failed");
        }

        _logger.LogDebug("Refresh token succeeded from {IpAddress}", ipAddress);
        return Ok(result);
    }


    /// <summary>
    /// Verify MFA code and complete authentication.
    /// This endpoint is called after initial login when MFA is enabled.
    /// Implements secure MFA verification (OWASP #2).
    /// </summary>
    /// <param name="request">MFA verification request containing temporary token and OTP code.</param>
    /// <returns>JWT access and refresh tokens upon successful MFA verification.</returns>
    [HttpPost("mfa/verify")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyMfa([FromBody] VerifyMfaRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? ApiConstants.Unknown;
        var command = _mapper.Map<VerifyMfaQuery>(request);
        command = command with { IpAddress = ipAddress };
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            _logger.LogDebug("MFA verification failed for user {UserId} from {IpAddress}", command.UserId, ipAddress);
            return ToProblem(result, "MFA Verification Failed");
        }

        _logger.LogDebug("MFA verification succeeded for user {UserId} from {IpAddress}", command.UserId, ipAddress);
        return Ok(result);
    }

    /// <summary>
    /// Forgot password endpoint to initiate password reset process.
    /// </summary>
    /// <param name="request">Forgot password request containing user email.</param>
    /// <returns>Success message if the reset email was sent.</returns>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? ApiConstants.Unknown;
        var template = await System.IO.File.ReadAllTextAsync(TemplateConstants.ResetPasswordTemplate);
        var command = _mapper.Map<ForgotPasswordCommand>(request);
        command = command with { Template = template, IpAddress = ipAddress };
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            _logger.LogDebug("Forgot password failed for {Email} from {IpAddress}", request.Email, ipAddress);
            return ToProblem(result, "Forgot Password Failed");
        }

        _logger.LogDebug("Forgot password succeeded for {Email} from {IpAddress}", request.Email, ipAddress);
        return Ok(result);
    }
    

    /// <summary>
    /// Reset password endpoint to complete password reset process.
    /// </summary>
    /// <param name="request">Reset password request containing reset token and new password.</param>
    /// <returns>Success message if the password was reset.</returns>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? ApiConstants.Unknown;
        var template = await System.IO.File.ReadAllTextAsync(TemplateConstants.PasswordUpdateTemplate);
        var command = _mapper.Map<ResetPasswordCommand>(request);
        command = command with { IpAddress = ipAddress, Template = template };
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            _logger.LogDebug("Reset password failed for {Email} from {IpAddress}", request.Email, ipAddress);
            return ToProblem(result, "Reset Password Failed");
        }

        _logger.LogDebug("Reset password succeeded for {Email} from {IpAddress}", request.Email, ipAddress);
        return Ok(result);
    }

}