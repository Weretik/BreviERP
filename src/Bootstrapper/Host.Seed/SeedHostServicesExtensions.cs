using Accounting.Infrastructure.DependencyInjection;
using BuildingBlocks.Infrastructure.DependencyInjection;
using Crm.Infrastructure.DependencyInjection;
using Identity.Infrastructure.Configuration;
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
        services.AddSeedIdentityConfiguration(configuration);
        services.AddIdentityInfrastructure(configuration);

        services.AddInfrastructureServices(configuration);

        services.AddReferenceDbContextServices(configuration);
        services.AddCrmDbContextServices(configuration);
        services.AddAccountingDbContextServices(configuration);

        return services;
    }

    private static IServiceCollection AddSeedIdentityConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<AdminUserConfig>(
            configuration.GetSection("Identity:AdminUser"));

        return services;
    }
}
