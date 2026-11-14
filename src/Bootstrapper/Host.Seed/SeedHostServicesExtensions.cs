using Crm.Infrastructure.DependencyInjection;
using Accounting.Infrastructure.DependencyInjection;
using BuildingBlocks.Infrastructure.DependencyInjection;
using Catalog.Infrastructure.DependencyInjection;
using Identity.Infrastructure.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Reference.Infrastructure.DependencyInjection;
using Reminders.Infrastructure.DependencyInjection;
using Media.Infrastructure.DependencyInjection;
using Production.Infrastructure.DependencyInjection;
using Purchasing.Infrastructure.DependencyInjection;
using Content.Infrastructure.DependencyInjection;
using Warehouse.Infrastructure.DependencyInjection;
using Ai.Infrastructure.DependencyInjection;
using CatalogImport.Persistence;
using BuildingBlocks.Infrastructure.Migrations;
using Microsoft.EntityFrameworkCore;
using SewingPricing.Jobs.Activation;
using Crm.Application.Tasks.Staff;

namespace Host.Seed;

public static class SeedHostServicesExtensions
{
    /// <summary>Container checks for every environment: no Scoped service inside a Singleton, every registration resolves.</summary>
    public static ServiceProviderOptions ServiceProviderOptions => new() { ValidateScopes = true, ValidateOnBuild = true };

    /// <summary>
    /// The modules' Contracts implementations send queries through Mediator, so the job host registers it as Host.Api does
    /// (without the HTTP pipeline behaviours): every registration in this container must resolve.
    /// </summary>
    private static void AddSeedMediator(this IServiceCollection services) =>
        services.AddMediator(options =>
        {
            options.ServiceLifetime = ServiceLifetime.Scoped;
            options.Assemblies =
            [
                typeof(Identity.Application.IdentityApplicationAssemblyMarker).Assembly,
                typeof(Catalog.Application.CatalogApplicationAssemblyMarker).Assembly,
                typeof(Crm.Application.CrmApplicationAssemblyMarker).Assembly,
                typeof(Reference.Application.ReferenceApplicationAssemblyMarker).Assembly,
                typeof(Reminders.Application.RemindersApplicationAssemblyMarker).Assembly,
                typeof(Media.Application.MediaApplicationAssemblyMarker).Assembly,
                typeof(Production.Application.ProductionApplicationAssemblyMarker).Assembly,
                typeof(Purchasing.Application.PurchasingApplicationAssemblyMarker).Assembly,
                typeof(Content.Application.ContentApplicationAssemblyMarker).Assembly,
                typeof(Warehouse.Application.WarehouseApplicationAssemblyMarker).Assembly,
                typeof(Accounting.Application.AccountingApplicationAssemblyMarker).Assembly,
                typeof(Ai.Application.AiApplicationAssemblyMarker).Assembly,
                typeof(BuildingBlocks.Application.ApplicationAssemblyMarker).Assembly
            ];
        });

    public static IServiceCollection AddSeedHostServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddIdentityInfrastructure(configuration);

        services.AddInfrastructureServices(configuration);

        services.AddReferenceDbContextServices(configuration);
        services.AddReferenceKeepinCrmSync(configuration);
        services.AddAccountingInfrastructure(configuration);
        services.AddCatalogDbContextServices(configuration);
        services.AddCrmDbContextServices(configuration);
        services.AddCrmKeepinCrmSync(configuration);
        services.AddRemindersInfrastructure(configuration);
        services.AddRemindersSeeders();
        services.AddMediaInfrastructure(configuration);
        // Warehouse before Production: its first migration takes over «Склад тканин» from the production schema.
        services.AddWarehouseInfrastructure(configuration);
        services.AddProductionInfrastructure(configuration);
        services.AddPurchasingInfrastructure(configuration);
        services.AddContentInfrastructure(configuration);
        services.AddAiInfrastructure(configuration);
        services.AddDbContext<CatalogImportDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Default")));
        services.AddScoped<IDatabaseMigrator, DbMigrator<CatalogImportDbContext>>();

        services.AddPricingActivationServices();
        services.AddSeedMediator();
        return services;
    }
}
