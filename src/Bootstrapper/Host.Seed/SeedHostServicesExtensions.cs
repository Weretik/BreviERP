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
using Ai.Infrastructure.DependencyInjection;
using CatalogImport.Persistence;
using BuildingBlocks.Infrastructure.Migrations;
using Microsoft.EntityFrameworkCore;
using SewingPricing.Jobs.Activation;
using Crm.Application.Tasks.Staff;
using Host.Api.Crm;

namespace Host.Seed;

public static class SeedHostServicesExtensions
{
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
        services.AddScoped<ITaskStaffReader, TaskStaffReader>();
        services.AddRemindersInfrastructure(configuration);
        services.AddRemindersSeeders();
        services.AddMediaInfrastructure(configuration);
        services.AddProductionInfrastructure(configuration);
        services.AddPurchasingInfrastructure(configuration);
        services.AddContentInfrastructure(configuration);
        services.AddAiInfrastructure(configuration);
        services.AddDbContext<CatalogImportDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Default")));
        services.AddScoped<IDatabaseMigrator, DbMigrator<CatalogImportDbContext>>();

        services.AddPricingActivationServices();
        return services;
    }
}
