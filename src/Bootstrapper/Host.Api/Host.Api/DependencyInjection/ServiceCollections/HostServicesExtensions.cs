using Accounting.Infrastructure.DependencyInjection;
using Crm.Infrastructure.DependencyInjection;
using Host.Api.DependencyInjection.ServiceCollections.HostServices;
using Identity.Infrastructure.DependencyInjection;
using Reference.Infrastructure.DependencyInjection;

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

        services.AddReferenceInfrastructureServices(configuration);
        services.AddCrmInfrastructureServices(configuration);
        services.AddAccountingInfrastructureServices(configuration);
        //services.AddReferenceApplicationServices(configuration);

        services.AddCorsService(configuration);

        services.AddMediatorPipeline();
        services.AddFluentValidation();

        services.AddModuleControllers();

        services.AddEndpointsApiExplorer();
        services.AddOpenApi();

        services.AddHealthChecks();
        services.AddSingleton(TimeProvider.System);

        services.AddAuthentication(configuration);

        services.AddProblemDetails();


        return services;
    }
}
