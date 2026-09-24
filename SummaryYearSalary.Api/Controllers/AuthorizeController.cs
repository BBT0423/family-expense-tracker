using SummaryYearSalary.Api.Helpers;
using SummaryYearSalary.Api.Models.Authorize;
using SummaryYearSalary.Application.Authorize.Commands.ForgotPasswordCommand;
using SummaryYearSalary.Application.Authorize.Commands.LoginCommand;
using SummaryYearSalary.Application.Authorize.Commands.RefreshTokenCommand;
using SummaryYearSalary.Application.Authorize.Commands.ResetPasswordCommand;
using SummaryYearSalary.Application.Authorize.Queries.VerifyMfaQuery;
using SummaryYearSalary.Application.Interfaces;
using SummaryYearSalary.Application.Interfaces.IServices;
using SummaryYearSalary.Infrastructure.Common;

using MapsterMapper;

using MediatR;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace SummaryYearSalary.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthorizeController(ISender mediator, IMapper mapper, IAppLogger logger, IAuthorizeService authorizeService, IOptions<GoogleAuthSettings> googleSettings) : BaseController
{
    private readonly ISender _mediator = mediator;
    private readonly IMapper _mapper = mapper;
    private readonly IAppLogger _logger = logger;
    private readonly IAuthorizeService _authorizeService = authorizeService;
    private readonly GoogleAuthSettings _googleSettings = googleSettings.Value;


    /// <summary>
    /// Authenticate user and return JWT tokens.
    /// </summary>
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
    /// </summary>
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
    /// </summary>
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

    /// <summary>
    /// Register a new user account.
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? ApiConstants.Unknown;
        var result = await _authorizeService.RegisterAsync(request.Email, request.Password, request.FirstName, request.LastName, ipAddress);

        if (!result.IsSuccess)
        {
            _logger.LogDebug("Registration failed for {Email} from {IpAddress}", request.Email, ipAddress);
            return ToProblem(result, "Registration Failed");
        }

        _logger.LogDebug("User {Email} registered successfully from {IpAddress}", request.Email, ipAddress);
        return Ok(result);
    }

    /// <summary>
    /// Initiate Google OAuth login - redirects to Google consent screen.
    /// </summary>
    [HttpGet("google")]
    [AllowAnonymous]
    public IActionResult GoogleLogin([FromQuery] string? returnUrl = null)
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = Url.Action(nameof(GoogleCallback), "Authorize"),
            Items = { { "returnUrl", returnUrl ?? "/" } }
        };
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    /// <summary>
    /// Google OAuth callback - exchanges code for user info and returns JWT.
    /// </summary>
    [HttpGet("google/callback")]
    [AllowAnonymous]
    public async Task<IActionResult> GoogleCallback()
    {
        var authenticateResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        if (!authenticateResult.Succeeded)
        {
            _logger.LogDebug("Google authentication failed");
            return Redirect($"{_googleSettings.CallbackUrl}?error=google_auth_failed");
        }

        var claims = authenticateResult.Principal?.Claims.ToList();
        var email = claims?.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value;
        var name = claims?.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Name)?.Value ?? string.Empty;
        var googleId = claims?.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

        if (string.IsNullOrEmpty(email))
        {
            return Redirect($"{_googleSettings.CallbackUrl}?error=no_email");
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? ApiConstants.Unknown;
        var result = await _authorizeService.GoogleLoginAsync(email, name, googleId, ipAddress);

        if (!result.IsSuccess)
        {
            _logger.LogDebug("Google login failed for {Email}: {Error}", email, result.Error);
            return Redirect($"{_googleSettings.CallbackUrl}?error={Uri.EscapeDataString(result.Error ?? "login_failed")}");
        }

        _logger.LogDebug("Google login succeeded for {Email} from {IpAddress}", email, ipAddress);

        var token = Uri.EscapeDataString(result.Value!.Token);
        var refreshToken = Uri.EscapeDataString(result.Value.RefreshToken);
        var userId = Uri.EscapeDataString(result.Value.User.Id);
        var userName = Uri.EscapeDataString(result.Value.User.Name ?? string.Empty);
        var userEmail = Uri.EscapeDataString(result.Value.User.Email ?? string.Empty);
        var userAvatar = Uri.EscapeDataString(result.Value.User.Avatar ?? string.Empty);
        var metadataJson = System.Text.Json.JsonSerializer.Serialize(result.Value.User.Metadata);
        var metadata = Uri.EscapeDataString(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(metadataJson)));
        return Redirect($"{_googleSettings.CallbackUrl}?token={token}&refreshToken={refreshToken}&userId={userId}&name={userName}&email={userEmail}&avatar={userAvatar}&metadata={metadata}");
    }

}
