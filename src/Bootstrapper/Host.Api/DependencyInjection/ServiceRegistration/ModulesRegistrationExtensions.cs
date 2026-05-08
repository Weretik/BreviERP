using Accounting.Infrastructure.DependencyInjection;
using Crm.Infrastructure.DependencyInjection;
using Host.Api.DependencyInjection.ServiceRegistration.Options;
using Identity.Infrastructure.DependencyInjection;
using Reference.Infrastructure.DependencyInjection;

namespace Host.Api.DependencyInjection.ServiceRegistration;

public static class ModuleRegistrationsExtensions
{
    public static IServiceCollection AddModuleRegistrations(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddIdentityConfiguration(configuration);
        services.AddIdentityInfrastructureServices(configuration);

        services.AddInfrastructureServices(configuration);

        services.AddReferenceInfrastructureServices(configuration);
        services.AddCrmInfrastructureServices(configuration);
        services.AddAccountingInfrastructureServices(configuration);

        return services;
    }
}
