using Catalog.Application.DependencyInjection;
using Host.Api.DependencyInjection.ServiceCollections.HostServices;

namespace Host.Api.DependencyInjection.ServiceCollections;

public static class HostServicesExtensions
{
    public static IServiceCollection AddHostServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddIdentityConfiguration(configuration);
        services.AddIdentityInfrastructure(configuration);

        services.AddInfrastructureServices(configuration);

        services.AddCatalogInfrastructureServices(configuration);
        services.AddCatalogApplicationServices(configuration);

        services.AddCorsService();

        services.AddMediatorPipeline();
        services.AddFluentValidation();

        services.AddModuleControllers();

        services.AddEndpointsApiExplorer();
        services.AddOpenApi();

        services.AddHealthChecks();
        services.AddSingleton(TimeProvider.System);

        services.AddAuthentication(configuration);

        return services;
    }
}
