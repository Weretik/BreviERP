using BuildingBlocks.Api.Outbox;
using Host.Api.DependencyInjection.Http;
using Host.Api.DependencyInjection.Modules;


namespace Host.Api.DependencyInjection;

public static class HostServicesExtensions
{
    public static IServiceCollection AddHostServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddModuleRegistrations(configuration);
        services.AddBackgroundJobRegistrations();
        services.AddApplicationRegistrations(configuration);
        services.AddApiRegistrations();
        services.AddSecurityRegistrations(configuration);
        services.AddOutboxDispatchEndpoint(configuration);
        return services;
    }
}
