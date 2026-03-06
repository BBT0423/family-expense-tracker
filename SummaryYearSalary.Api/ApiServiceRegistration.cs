using SummaryYearSalary.Api.Configurations;

using Microsoft.AspNetCore.Mvc;

using QuestPDF.Infrastructure;

namespace SummaryYearSalary.Api;

internal static class ApiServiceRegistration
{
    public static IServiceCollection AddApiServiceRegistration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddControllers()
                .ConfigureApiBehaviorOptions(options => options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState
                        .Where(e => e.Value?.Errors.Count > 0)
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value!.Errors.Select(x => x.ErrorMessage).ToArray()
                        );

                    // Build detail string with field names and their errors
                    var detail = string.Join(", ",
                        errors.Select(kvp => $"{kvp.Key}: {string.Join("; ", kvp.Value)}")
                    );

                    var problemDetails = new ValidationProblemDetails(errors)
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Title = "One or more validation errors occurred.",
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                        Instance = context.HttpContext.Request.Path,
                        Detail = detail
                    };
                    return new BadRequestObjectResult(problemDetails)
                    {
                        ContentTypes = { "application/problem+json" }
                    };
                });

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        services.AddMappingConfigurations();
        services.AddMemoryCache();

        // Configure QuestPDF license
        QuestPDF.Settings.License = LicenseType.Community;

        return services;
    }

}