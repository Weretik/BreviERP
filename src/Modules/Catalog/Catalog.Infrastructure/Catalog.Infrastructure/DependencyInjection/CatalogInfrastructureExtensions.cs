using Catalog.Contracts.Pricing;
using Catalog.Contracts.Products;
using Catalog.Contracts.Sewing;
using Catalog.Infrastructure.Adapters.Media;
using Catalog.Infrastructure.Contracts.Pricing;
using Catalog.Infrastructure.Contracts.Products;
using Catalog.Infrastructure.Contracts.Sewing;
using Catalog.Infrastructure.Adapters.Reference;
using Microsoft.Extensions.DependencyInjection.Extensions;
using BuildingBlocks.Infrastructure.Migrations;
using BuildingBlocks.Infrastructure.Seeding;
using Catalog.Application.Abstractions;
using Catalog.Application.Abstractions.Media;
using Catalog.Application.Abstractions.Pricing;
using Catalog.Application.Abstractions.Reference;
using Catalog.Infrastructure.Persistence;
using Catalog.Infrastructure.Persistence.Pricing;
using Catalog.Infrastructure.Persistence.Products;
using Catalog.Infrastructure.Persistence.Seeding.ProductCategories;

namespace Catalog.Infrastructure.DependencyInjection;

public static class CatalogInfrastructureExtensions
{
    public static IServiceCollection AddCatalogInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
                               ?? throw new InvalidOperationException("Missing ConnectionStrings:Default");

        services.TryAddScoped<PricingInputChanges>();
        services.AddScoped<SewingPricingInputChangeInterceptor>();
        services.TryAddSingleton(TimeProvider.System);
        services.AddDbContext<CatalogDbContext>((provider, options) => options.UseNpgsql(connectionString)
            .AddInterceptors(provider.GetRequiredService<SewingPricingInputChangeInterceptor>()));

        services.AddScoped(typeof(ICatalogRepository<>), typeof(CatalogEfRepository<>));
        services.AddScoped(typeof(ICatalogReadRepository<>), typeof(CatalogReadEfRepository<>));
        services.AddScoped<Catalog.Application.Abstractions.ISewingCompositionStore, SewingCompositionStore>();
        services.AddScoped<SewingCompositionReadStore>();
        services.AddScoped<Catalog.Application.Abstractions.ISewingCompositionReadStore>(provider => provider.GetRequiredService<SewingCompositionReadStore>());
        services.AddScoped<Catalog.Application.Abstractions.ISewingCompositionListReader>(provider => provider.GetRequiredService<SewingCompositionReadStore>());
        services.AddScoped<IDatabaseMigrator, DbMigrator<CatalogDbContext>>();
        services.AddScoped<Catalog.Application.Abstractions.Pricing.ISewingPricingInputReader, Catalog.Infrastructure.Persistence.Pricing.Sewing.SewingPricingInputReader>();
        services.AddScoped<Catalog.Application.Abstractions.Pricing.ISewingPricingSettingsReader, Catalog.Infrastructure.Persistence.Pricing.Sewing.SewingPricingSettingsReader>();
        services.AddScoped<ISeeder, ProductCategorySeeder>();
        services.AddScoped<Catalog.Application.Abstractions.Pricing.ISewingPricingSettingsWriter, Catalog.Infrastructure.Persistence.Pricing.Sewing.SewingPricingSettingsWriter>();
        services.AddScoped<Catalog.Application.Abstractions.Pricing.ISewingPricingTransaction, Catalog.Infrastructure.Persistence.Pricing.SewingPricingTransaction>();
        services.AddScoped<Catalog.Application.Abstractions.Pricing.ISewingCalculationStore, Catalog.Infrastructure.Persistence.Pricing.Calculations.SewingCalculationStore>();
        services.AddScoped<Catalog.Application.Abstractions.Pricing.ISewingPublicationStore, Catalog.Infrastructure.Persistence.Pricing.Publications.SewingPublicationStore>();
        services.AddScoped<Catalog.Application.Abstractions.Pricing.ISewingPublishedPriceReader, Catalog.Infrastructure.Persistence.Pricing.Publications.SewingPublishedPriceReader>();

        services.AddScoped<Catalog.Application.Features.Pricing.Shared.Sewing.AutoPublication.SewingPriceStatusReader>();
        services.AddScoped<Catalog.Application.Features.Pricing.Shared.Sewing.AutoPublication.SewingPricePublisher>();
        services.AddScoped<Catalog.Application.Abstractions.IProductOverviewVariantReader, Catalog.Infrastructure.Persistence.Products.ProductOverviewVariantReader>();
        services.AddScoped<Catalog.Application.Features.Product.Shared.Overview.Items.ProductOverviewItemReader>();

        services.AddScoped<Catalog.Application.Abstractions.Pricing.IPpePricingInputReader, Catalog.Infrastructure.Persistence.Pricing.Ppe.PpePricingInputReader>();
        services.AddScoped<Catalog.Application.Abstractions.Pricing.IPpeRuleVersionWriter, Catalog.Infrastructure.Persistence.Pricing.Ppe.PpeRuleVersionWriter>();
        services.AddScoped<Catalog.Application.Abstractions.Pricing.IPpeOfferVersionWriter, Catalog.Infrastructure.Persistence.Pricing.Ppe.PpeOfferVersionWriter>();
        services.AddScoped<Catalog.Application.Abstractions.ISheetProductImportStore, Catalog.Infrastructure.Persistence.Products.SheetProductImportStore>();
        services.AddScoped<Catalog.Application.Abstractions.Pricing.IPpePriceSourceStore, Catalog.Infrastructure.Persistence.Pricing.PpeSupplierPrices.PpePriceSourceStore>();
        services.AddScoped<Catalog.Application.Abstractions.Pricing.IPpeSupplierFileReader, Catalog.Infrastructure.Integrations.PpeSupplierFiles.PpeSupplierFileReader>();
        services.AddHttpClient<Catalog.Infrastructure.Integrations.PpeSupplierSites.SupplierPageClient>(client => client.Timeout = TimeSpan.FromSeconds(30));
        services.Configure<Catalog.Infrastructure.Integrations.PpeSupplierSites.Cabinets.SupplierCabinetOptions>(
            configuration.GetSection(Catalog.Infrastructure.Integrations.PpeSupplierSites.Cabinets.SupplierCabinetOptions.Section));
        services.AddHttpClient<Catalog.Infrastructure.Integrations.PpeSupplierSites.Cabinets.SupplierCabinetClient>(client => client.Timeout = TimeSpan.FromSeconds(30))
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AllowAutoRedirect = false, UseCookies = false });
        services.AddScoped<Catalog.Infrastructure.Integrations.PpeSupplierSites.Cabinets.SupplierCabinetPriceReader>();
        services.AddScoped<Catalog.Application.Abstractions.Pricing.IPpeSupplierOnlinePriceReader, Catalog.Infrastructure.Integrations.PpeSupplierSites.PpeSupplierOnlinePriceReader>();
        services.AddScoped<Catalog.Application.Features.Pricing.Shared.Ppe.SupplierPrices.Running.PpePriceImportRunner>();
        services.AddScoped<Catalog.Application.Features.Pricing.Commands.RefreshPpeSupplierPrices.PpeScheduledPriceRefresh>();

        // Adapters from other modules' Contracts to this module's ports.
        services.AddScoped<IFabricNameReader, FabricNameReader>();
        services.AddScoped<IProductSectionReader, ProductSectionReader>();
        services.AddScoped<IProductReferenceCodeReader, ProductReferenceCodeReader>();
        services.AddScoped<IProductReferenceReader, ProductReferenceReader>();
        services.AddScoped<IPricingReferenceVersionReader, PricingReferenceVersionReader>();
        services.AddScoped<IProductMediaReader, ProductMediaReader>();
        services.AddScoped<IProductPhotoSource, ProductPhotoSource>();
        services.AddSingleton<IProductSlugGenerator, ProductSlugGenerator>();

        // Catalog.Contracts for other modules.
        services.AddScoped<ICatalogProductReader, CatalogProductReader>();
        services.AddScoped<ICatalogUsage, CatalogUsage>();
        services.AddScoped<ISewingModelReader, SewingModelReader>();
        services.AddScoped<ISewingPriceCalculator, SewingPriceCalculator>();
        services.AddScoped<IPricingSettingsSync, PricingSettingsSync>();
        return services;
    }
}
