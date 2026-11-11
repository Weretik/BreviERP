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
using Host.Api.Purchasing;
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
        // Before Identity, which falls back to switched-off avatars when nothing is registered.
        services.AddSingleton<Identity.Application.Services.IAvatarStorage, IdentityAvatarStorage>();
        services.AddIdentityInfrastructureServices(configuration);

        services.AddInfrastructureServices(configuration);

        services.AddReferenceInfrastructureServices(configuration);
        services.AddAccountingInfrastructure(configuration);
        services.AddScoped<IAccountingCustomerReader, AccountingCustomerReader>();
        services.AddScoped<IAccountingSupplierReader, AccountingSupplierReader>();
        services.AddCatalogInfrastructureServices(configuration);
        services.AddCrmDbContextServices(configuration);
        services.AddCrmKeepinCrmSync(configuration);
        services.AddScoped<IQuoteCatalogReader, QuoteCatalogReader>();
        services.AddScoped<IQuoteFabricFactsReader, QuoteFabricFactsReader>();
        services.AddScoped<IQuoteProductionOrders, QuoteProductionOrders>();
        services.AddScoped<IQuotePhotoStorage, QuotePhotoStorage>();
        services.AddScoped<IQuoteSewingCalculator, QuoteSewingCalculator>();
        services.AddScoped<IQuoteModelCompositionReader, QuoteModelCompositionReader>();
        services.AddScoped<IQuoteStaffReader, QuoteStaffReader>();
        services.AddScoped<IQuoteImportLookup, QuoteImportLookup>();
        services.AddScoped<ITaskStaffReader, TaskStaffReader>();
        services.AddScoped<IEmbroideryStitchRateReader, EmbroideryStitchRateReader>();
        services.AddScoped<IEmbroideryMediaReader, EmbroideryMediaReader>();
        services.AddRemindersInfrastructure(configuration);
        services.AddMediaInfrastructure(configuration);
        services.AddPurchasingInfrastructure(configuration);
        services.AddContentInfrastructure(configuration);
        services.AddScoped<IContentCatalogReader, ContentCatalogReader>();
        services.AddScoped<IPurchasingSupplierReader, PurchasingSupplierReader>();
        services.AddScoped<IPurchasingPaymentReader, PurchasingPaymentReader>();
        // Recognition runs only in the API: the recognizer reaches the Ai module through Mediator.
        services.AddScoped<IPurchaseDocumentRecognizer, PurchaseDocumentRecognizer>();
        services.AddSingleton<global::Ai.Application.Abstractions.IAiBasePrompt, PurchaseInvoiceBasePrompt>();
        services.AddScoped<global::Purchasing.Application.Documents.Recognize.PurchaseFileRecognition>();
        services.AddAiInfrastructure(configuration);
        services.AddScoped<IProductMediaReader, CatalogProductMediaReader>();
        services.AddHttpClient<IProductPhotoSource, CatalogProductPhotoSource>(client => client.Timeout = TimeSpan.FromSeconds(30));
        services.AddScoped<IMediaUsageReader, MediaProductUsageReader>();
        services.AddScoped<IProductReferenceReader, CatalogProductReferenceReader>();
        services.AddScoped<IProductSectionReader, CatalogProductSectionReader>();
        services.AddScoped<IFabricNameReader, CatalogFabricNameReader>();
        services.AddScoped<IProductReferenceCodeReader, CatalogProductReferenceCodeReader>();
        services.AddScoped<Catalog.Application.Contracts.Pricing.IPricingReferenceVersionReader, CatalogPricingReferenceReader>();
        services.AddScoped<IProductUsageReader, CatalogProductUsageReader>();
        services.AddScoped<IPricingSettingsChangeNotifier, CatalogPricingSettingsSync>();
        // Warehouse before Production: its first migration takes over «Склад тканин» from the production schema.
        services.AddWarehouseInfrastructure(configuration);
        services.AddScoped<IWarehouseReferenceReader, WarehouseReferenceReader>();
        services.AddScoped<IWarehouseStaffReader, WarehouseStaffReader>();
        services.AddScoped<IWarehouseProductionReader, WarehouseProductionReader>();
        services.AddProductionInfrastructure(configuration);
        services.AddScoped<IProductionModelReader, ProductionModelReader>();
        services.AddScoped<IProductionQuoteReader, ProductionQuoteReader>();
        services.AddScoped<IProductionEmbroideryReader, ProductionEmbroideryReader>();
        services.AddScoped<IProductionFabricReader, ProductionFabricReader>();
        services.AddScoped<IProductionColorReader, ProductionColorReader>();
        services.AddScoped<IProductionStockReader, ProductionStockReader>();
        services.AddScoped<IProductionStaffReader, ProductionStaffReader>();
        services.AddScoped<IPayrollParameterReader, PayrollParameterReader>();
        services.AddScoped<IProductionFileStorage, ProductionFileStorage>();
        services.AddScoped<IProductionTelegram, ProductionTelegram>();
        services.AddSingleton<IProductSlugGenerator, ProductSlugGenerator>();

        return services;
    }
}
