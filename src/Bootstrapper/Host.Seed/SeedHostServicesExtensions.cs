using Crm.Infrastructure.DependencyInjection;
using Accounting.Infrastructure.DependencyInjection;
using BuildingBlocks.Infrastructure.DependencyInjection;
using Catalog.Infrastructure.DependencyInjection;
using Identity.Infrastructure.Configuration;
using Identity.Infrastructure.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Reference.Infrastructure.DependencyInjection;
using CatalogImport.Persistence;
using BuildingBlocks.Infrastructure.Migrations;
using Microsoft.EntityFrameworkCore;
using SewingPricing.Jobs.Activation;

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
        services.AddAccountingDbContextServices(configuration);
        services.AddCatalogDbContextServices(configuration);
        services.AddCrmDbContextServices(configuration);
        services.AddDbContext<CatalogImportDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Default")));
        services.AddScoped<IDatabaseMigrator, DbMigrator<CatalogImportDbContext>>();

        services.AddPricingActivationServices();
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
