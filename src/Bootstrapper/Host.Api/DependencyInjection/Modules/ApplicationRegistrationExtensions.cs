

namespace Host.Api.DependencyInjection.Modules;

public static class ApplicationRegistrationsExtensions
{
    public static IServiceCollection AddApplicationRegistrations(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMediatorPipeline();

        return services;
    }
}
