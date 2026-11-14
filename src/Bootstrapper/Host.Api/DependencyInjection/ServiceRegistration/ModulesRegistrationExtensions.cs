using Crm.Application.SiteRequests.Abstractions;
using Crm.Application.Embroidery.Persistence;
using Crm.Application.Embroidery.StitchRate;
using Crm.Application.Embroidery.Mockup;
using Crm.Application.Quotes.Catalog;
using Crm.Application.Quotes.FabricFacts;
using Crm.Application.Quotes.Photos;
using Crm.Application.Quotes.ProductionLinks;
using Crm.Application.Quotes.Import;
using Crm.Application.Quotes.CustomSewing;
using Crm.Application.Quotes.Staff;
using Crm.Application.Tasks.Staff;
using Crm.Infrastructure.DependencyInjection;
using Host.Api.Crm;
using Accounting.Application.Abstractions;
using Accounting.Infrastructure.DependencyInjection;
using Host.Api.Accounting;
using Catalog.Application.Contracts.Reference;
using Catalog.Application.Contracts.SlugGeneration;
using Catalog.Infrastructure.Products.SlugGeneration;
using Reference.Application.Contracts.Catalog;
using Catalog.Infrastructure.DependencyInjection;
using Host.Api.DependencyInjection.ServiceRegistration.Options;
using Identity.Infrastructure.DependencyInjection;
using Reference.Infrastructure.DependencyInjection;
using Reminders.Infrastructure.DependencyInjection;
using Catalog.Application.Contracts.Media;
using Media.Application.Abstractions;
using Media.Infrastructure.DependencyInjection;
using Host.Api.Production;
using Production.Application.Abstractions;
using Production.Infrastructure.DependencyInjection;
using Purchasing.Application.Abstractions;
using Purchasing.Infrastructure.DependencyInjection;
using Content.Application.Abstractions;
using Content.Infrastructure.DependencyInjection;
using Host.Api.Content;
using Host.Api.Warehouse;
using Warehouse.Application.Abstractions;
using Warehouse.Infrastructure.DependencyInjection;
using Ai.Infrastructure.DependencyInjection;

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
        services.AddAccountingInfrastructure(configuration);
        services.AddScoped<IAccountingCustomerReader, AccountingCustomerReader>();
        services.AddCatalogInfrastructureServices(configuration);
        services.AddCrmDbContextServices(configuration);
        services.AddCrmKeepinCrmSync(configuration);
        services.AddScoped<IQuoteCatalogReader, QuoteCatalogReader>();
        services.AddScoped<IQuoteProductionOrders, QuoteProductionOrders>();
        services.AddScoped<IQuoteSewingCalculator, QuoteSewingCalculator>();
        services.AddScoped<IQuoteModelCompositionReader, QuoteModelCompositionReader>();
        services.AddScoped<IQuoteImportLookup, QuoteImportLookup>();
        services.AddRemindersInfrastructure(configuration);
        services.AddMediaInfrastructure(configuration);
        services.AddPurchasingInfrastructure(configuration);
        services.AddContentInfrastructure(configuration);
        services.AddScoped<IContentCatalogReader, ContentCatalogReader>();
        services.AddAiInfrastructure(configuration);
        services.AddScoped<IMediaUsageReader, MediaProductUsageReader>();
        services.AddScoped<IProductUsageReader, CatalogProductUsageReader>();
        services.AddScoped<IPricingSettingsChangeNotifier, CatalogPricingSettingsSync>();
        // Warehouse before Production: its first migration takes over «Склад тканин» from the production schema.
        services.AddWarehouseInfrastructure(configuration);
        services.AddScoped<IWarehouseProductionReader, WarehouseProductionReader>();
        services.AddProductionInfrastructure(configuration);
        services.AddScoped<IProductionModelReader, ProductionModelReader>();
        services.AddScoped<IProductionQuoteReader, ProductionQuoteReader>();
        services.AddScoped<IProductionEmbroideryReader, ProductionEmbroideryReader>();
        services.AddSingleton<IProductSlugGenerator, ProductSlugGenerator>();

        return services;
    }
}
