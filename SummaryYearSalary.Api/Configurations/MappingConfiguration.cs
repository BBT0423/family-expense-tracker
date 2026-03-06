using System.Reflection;

using Mapster;

using MapsterMapper;

namespace SummaryYearSalary.Api.Configurations;

internal static class MappingConfiguration
{
    public static IServiceCollection AddMappingConfigurations(this IServiceCollection services)
    {
        var config = TypeAdapterConfig.GlobalSettings;
        config.Scan(Assembly.GetExecutingAssembly());
        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();
        return services;
    }
}