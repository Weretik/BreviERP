using Crm.Application.Embroidery.Persistence;
using Crm.Application.Embroidery.StitchRate;
using Crm.Application.Embroidery.Mockup;
using Crm.Application.Quotes.Catalog;
using Crm.Application.Quotes.Import;
using Crm.Application.Quotes.CustomSewing;
using Crm.Application.Quotes.Staff;
using Crm.Application.Tasks.Staff;
using Crm.Infrastructure.DependencyInjection;
using Host.Api.Crm;
using Accounting.Infrastructure.DependencyInjection;
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
using Purchasing.Infrastructure.DependencyInjection;

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
        services.AddAccountingInfrastructureServices(configuration);
        services.AddCatalogInfrastructureServices(configuration);
        services.AddCrmDbContextServices(configuration);
        services.AddCrmKeepinCrmSync(configuration);
        services.AddScoped<IQuoteCatalogReader, QuoteCatalogReader>();
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
        services.AddProductionInfrastructure(configuration);
        services.AddScoped<IProductionModelReader, ProductionModelReader>();
        services.AddScoped<IProductionQuoteReader, ProductionQuoteReader>();
        services.AddScoped<IProductionEmbroideryReader, ProductionEmbroideryReader>();
        services.AddScoped<IProductionFabricReader, ProductionFabricReader>();
        services.AddScoped<IProductionSupplierReader, ProductionSupplierReader>();
        services.AddScoped<IProductionStaffReader, ProductionStaffReader>();
        services.AddScoped<IPayrollParameterReader, PayrollParameterReader>();
        services.AddScoped<IProductionFileStorage, ProductionFileStorage>();
        services.AddScoped<IProductionTelegram, ProductionTelegram>();
        services.AddSingleton<IProductSlugGenerator, ProductSlugGenerator>();

        return services;
    }
}
