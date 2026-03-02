using Accounting.Infrastructure.DependencyInjection;
using BuildingBlocks.Infrastructure.DependencyInjection;
using Crm.Infrastructure.DependencyInjection;
using Host.Api.DependencyInjection.ServiceCollections.HostServices;
using Identity.Infrastructure.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Reference.Infrastructure.DependencyInjection;

namespace Host.Seed;

public static class SeedHostServicesExtensions
{
    public static IServiceCollection AddSeedHostServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddIdentityConfiguration(configuration);
        services.AddIdentityInfrastructure(configuration);

        services.AddInfrastructureServices(configuration);

        services.AddReferenceDbContextServices(configuration);
        services.AddCrmDbContextServices(configuration);
        services.AddAccountingDbContextServices(configuration);

        return services;
    }
}
