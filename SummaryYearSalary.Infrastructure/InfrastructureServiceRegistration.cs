using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.RateLimiting;

using SummaryYearSalary.Application.Interfaces;
using SummaryYearSalary.Application.Interfaces.IRepositories;
using SummaryYearSalary.Application.Interfaces.IServices;
using SummaryYearSalary.Domain.Entities;
using SummaryYearSalary.Infrastructure.Common;
using SummaryYearSalary.Infrastructure.Data;
using SummaryYearSalary.Infrastructure.Repositories;
using SummaryYearSalary.Infrastructure.Services;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

using Serilog;

namespace SummaryYearSalary.Infrastructure;

public static class InfrastructureServiceRegistration
{
    private static readonly string ConstantsUnknown = "Unknown";

    public static IServiceCollection AddInfrastructureServiceRegistration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var emailSettings = services.ConfigureOptions(configuration);
        services.AddDatabaseConfiguration(configuration);
        services.AddIdentityConfiguration();
        services.AddAuthenticationConfiguration(configuration);
        services.AddRateLimitingConfiguration(configuration);
        services.AddCorsConfiguration(configuration);
        services.AddEmailConfiguration(emailSettings);
        services.AddRepositories();

        return services;
    }

    private static EmailSettings ConfigureOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<SecuritySettings>(configuration.GetSection(SecuritySettings.SectionName));
        services.Configure<RateLimitSettings>(configuration.GetSection(RateLimitSettings.SectionName));

        var emailConfiguration = configuration.GetSection(EmailSettings.SectionName).Get<EmailSettings>() ?? new EmailSettings();
        var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>() ?? new JwtSettings();
        var securitySettings = configuration.GetSection(SecuritySettings.SectionName).Get<SecuritySettings>() ?? new SecuritySettings();
        var innoSmtpUsername = Environment.GetEnvironmentVariable("INNO_SMTP_USERNAME") ?? string.Empty;
        var innoSmtpPassword = Environment.GetEnvironmentVariable("INNO_SMTP_PASSWORD") ?? string.Empty;
        var innoJwtSecret = Environment.GetEnvironmentVariable("INNO_JWT_SECRET") ?? string.Empty;

        jwtSettings.Secret = jwtSettings.Secret?.Replace("{INNO_JWT_SECRET}", innoJwtSecret, StringComparison.Ordinal) ?? string.Empty;
        services.Configure<JwtSettings>(options =>
        {
            options.Issuer = jwtSettings.Issuer;
            options.Audience = jwtSettings.Audience;
            options.Secret = jwtSettings.Secret;
            options.AccessTokenExpirationMinutes = jwtSettings.AccessTokenExpirationMinutes;
            options.RefreshTokenExpirationDays = jwtSettings.RefreshTokenExpirationDays;
            options.ResetPasswordTokenExpiresMinutes = jwtSettings.ResetPasswordTokenExpiresMinutes;
        });

        emailConfiguration.From = emailConfiguration.From?.Replace("{INNO_SMTP_USERNAME}", innoSmtpUsername, StringComparison.Ordinal) ?? string.Empty;
        emailConfiguration.UserName = emailConfiguration.UserName?.Replace("{INNO_SMTP_USERNAME}", innoSmtpUsername, StringComparison.Ordinal) ?? string.Empty;
        emailConfiguration.Password = emailConfiguration.Password?.Replace("{INNO_SMTP_PASSWORD}", innoSmtpPassword, StringComparison.Ordinal) ?? string.Empty;
        services.Configure<EmailSettings>(options =>
        {
            options.From = emailConfiguration.From;
            options.SmtpServer = emailConfiguration.SmtpServer;
            options.Port = emailConfiguration.Port;
            options.UserName = emailConfiguration.UserName;
            options.Password = emailConfiguration.Password;
            options.EnableEmailNotifications = emailConfiguration.EnableEmailNotifications;
        });

        // Configure consolidated settings for AuthorizeService
        services.Configure<AuthorizeServiceSettings>(options =>
        {
            options.Security = securitySettings;
            options.Jwt = jwtSettings;
            options.Email = emailConfiguration;
        });

        return emailConfiguration;
    }

    private static IServiceCollection AddDatabaseConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("SummaryYearSalaryConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("The connection string 'SummaryYearSalaryConnection' is missing or empty. The application cannot start without a valid database connection string.");
        }

        // Replace environment variable placeholders using environment variables
        if (connectionString.Contains("{INNO_DB_CONNECTION}", StringComparison.Ordinal))
        {
            var innoDbConnection = Environment.GetEnvironmentVariable("INNO_DB_CONNECTION");
            if (string.IsNullOrWhiteSpace(innoDbConnection))
            {
                throw new InvalidOperationException("The environment variable 'INNO_DB_CONNECTION' is missing or empty, but the connection string contains the '{INNO_DB_CONNECTION}' placeholder. The application cannot start without a valid database connection configuration.");
            }

            connectionString = connectionString.Replace("{INNO_DB_CONNECTION}", innoDbConnection, StringComparison.Ordinal);
        }
        services.AddDbContext<SummaryYearSalaryDbContext>(options =>
        {
            options.UseSqlServer(connectionString,
                sqlOptions => sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorNumbersToAdd: null));

            // Enable sensitive data logging only in development
            if (configuration.GetValue<bool>("IsDevelopment"))
            {
                options.EnableSensitiveDataLogging();
            }
        });

        return services;
    }

    private static IServiceCollection AddIdentityConfiguration(
        this IServiceCollection services)
    {
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            // Password settings
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 8;
            options.Password.RequiredUniqueChars = 4;

            // Lockout settings
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            // User settings
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = false; // Set to true in production
            options.SignIn.RequireConfirmedAccount = false;
        })
        .AddEntityFrameworkStores<SummaryYearSalaryDbContext>()
        .AddDefaultTokenProviders();

        return services;
    }

    private static IServiceCollection AddAuthenticationConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>();
        var key = Encoding.UTF8.GetBytes(jwtSettings!.Secret);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.RequireHttpsMetadata = true; // Enforce HTTPS
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.Zero // Reduce clock skew for tighter expiration
            };

            // Configure events for logging
            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    Log.Warning("JWT authentication failed: {Error}", context.Exception.Message);
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    Log.Information("JWT token validated for user: {User}", context.Principal?.Identity?.Name);
                    return Task.CompletedTask;
                }
            };
        });

        return services;
    }

    private static IServiceCollection AddRateLimitingConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var rateLimitSettings = configuration.GetSection(RateLimitSettings.SectionName).Get<RateLimitSettings>();

        services.AddRateLimiter(options =>
        {
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            {
                var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? ConstantsUnknown;
                var partitionKey = context.User.Identity?.IsAuthenticated == true ? context.User.Identity.Name! : ipAddress;

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: partitionKey,
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = rateLimitSettings!.PermitLimit,
                        Window = TimeSpan.FromSeconds(rateLimitSettings.WindowSeconds),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = rateLimitSettings.QueueLimit
                    });
            });

            options.OnRejected = async (context, cancellationToken) =>
            {
                var problemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Title = "Too many requests",
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.8",
                    Detail = "You have exceeded the allowed number of requests. Please try again later.",
                    Instance = context.HttpContext.Request.Path
                };
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                Log.Warning("Rate limit exceeded for {IpAddress}",
                    context.HttpContext.Connection.RemoteIpAddress);

                context.HttpContext.Response.ContentType = "application/problem+json";
                await context.HttpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken: cancellationToken);
            };
        });

        return services;
    }

    private static IServiceCollection AddCorsConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddCors(options => options.AddDefaultPolicy(builder => builder
            .AllowAnyHeader()
            .WithMethods("GET", "POST", "PUT", "DELETE")
            .WithOrigins(configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [])
            .AllowCredentials()));

        return services;
    }

    private static IServiceCollection AddEmailConfiguration(
        this IServiceCollection services,
        EmailSettings emailSettings)
    {
        if (!emailSettings.EnableEmailNotifications)
        {
            return services;
        }

        services.AddFluentEmail(emailSettings.From, defaultFromName: "SummaryYearSalary Web")
                .AddSmtpSender(() => new SmtpClient(emailSettings.SmtpServer)
                {
                    Port = emailSettings.Port ?? 587,
                    Credentials = new NetworkCredential(emailSettings.UserName, emailSettings.Password),
                    EnableSsl = true,
                });

        services.AddSingleton<EmailRetryHandler>();
        services.AddScoped<IEmailRepository, EmailRepository>();
        services.AddHostedService<EmailRecoveryBackgroundService>();

        return services;
    }

    private static IServiceCollection AddRepositories(
        this IServiceCollection services)
    {

        // Generic repository and unit of work
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Logging and audit
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IAppLogger, AppLogger>();

        // Authorization and OTP services
        services.AddScoped<IAuthorizeService, AuthorizeService>();
        services.AddScoped<IOtpService, OtpService>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        return services;
    }
}