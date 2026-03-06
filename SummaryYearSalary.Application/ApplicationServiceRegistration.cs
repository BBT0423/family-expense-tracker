using SummaryYearSalary.Application.Common;

using FluentValidation;

using Microsoft.Extensions.DependencyInjection;

namespace SummaryYearSalary.Application;

public static class ApplicationServiceRegistration
{
    public static IServiceCollection AddApplicationServiceRegistration(
        this IServiceCollection services)
    {
        services.AddMediatR(options =>
            {
                options.RegisterServicesFromAssembly(typeof(ApplicationServiceRegistration).Assembly);
                options.AddOpenBehavior(typeof(ValidationBehavior<,>));
            });

        services.AddValidatorsFromAssemblyContaining(typeof(ApplicationServiceRegistration));

        return services;
    }
}