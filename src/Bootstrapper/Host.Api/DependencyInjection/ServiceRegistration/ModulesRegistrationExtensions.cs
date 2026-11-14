using Accounting.Api.DependencyInjection;
using Ai.Api.DependencyInjection;
using Catalog.Api.DependencyInjection;
using Content.Api.DependencyInjection;
using Crm.Api.DependencyInjection;
using Identity.Api.DependencyInjection;
using Media.Api.DependencyInjection;
using Production.Api.DependencyInjection;
using Purchasing.Api.DependencyInjection;
using Reference.Api.DependencyInjection;
using Reminders.Api.DependencyInjection;
using Warehouse.Api.DependencyInjection;

namespace Host.Api.DependencyInjection.ServiceRegistration;

public static class ModuleRegistrationsExtensions
{
    /// <summary>Shared building blocks, then one call per module; a module registers its own layers and adapters.</summary>
    public static IServiceCollection AddModuleRegistrations(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddIdentityModule(configuration);
        services.AddReferenceModule(configuration);
        services.AddAccountingModule(configuration);
        services.AddCatalogModule(configuration);
        services.AddCrmModule(configuration);
        services.AddRemindersModule(configuration);
        services.AddMediaModule(configuration);
        services.AddPurchasingModule(configuration);
        services.AddContentModule(configuration);
        services.AddAiModule(configuration);
        // Warehouse before Production: its first migration takes over «Склад тканин» from the production schema.
        services.AddWarehouseModule(configuration);
        services.AddProductionModule(configuration);

        return services;
    }

    /// <summary>One call per module that has moved from controllers to Minimal API endpoints.</summary>
    public static IEndpointRouteBuilder MapModuleEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapAiEndpoints();
        app.MapPurchasingEndpoints();
        app.MapWarehouseEndpoints();
        app.MapRemindersEndpoints();
        app.MapMediaEndpoints();
        app.MapIdentityEndpoints();
        app.MapReferenceEndpoints();
        app.MapProductionEndpoints();
        app.MapAccountingEndpoints();
        app.MapCrmEndpoints();
        app.MapCatalogEndpoints();
        app.MapContentEndpoints();

        return app;
    }
}
