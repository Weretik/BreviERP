using Catalog.Contracts.Pricing;
using Catalog.Contracts.Products;
using Catalog.Contracts.Sewing;
using Catalog.Infrastructure.Adapters.Media;
using Catalog.Infrastructure.Contracts.Pricing;
using Catalog.Infrastructure.Contracts.Products;
using Catalog.Infrastructure.Contracts.Sewing;
using Catalog.Infrastructure.Products.SlugGeneration;
using Catalog.Infrastructure.Adapters.Reference;
using Microsoft.Extensions.DependencyInjection.Extensions;
using BuildingBlocks.Infrastructure.Migrations;
using BuildingBlocks.Infrastructure.Seeding;
using Catalog.Infrastructure.DataBase;
using Catalog.Infrastructure.Pricing.AutoPublication;
using Catalog.Infrastructure.Repositories;
using Catalog.Infrastructure.Seeders.ProductCategories;
using Catalog.Application.Abstractions;
using Catalog.Application.Abstractions.Media;
using Catalog.Application.Abstractions.Pricing;
using Catalog.Application.Abstractions.Reference;

namespace Catalog.Infrastructure.DependencyInjection;

public static class CatalogDbContextExtensions
{
    public static IServiceCollection AddCatalogDbContextServices(
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
        services.AddScoped<Catalog.Application.Abstractions.Pricing.ISewingPricingInputReader, Catalog.Infrastructure.Pricing.Reading.SewingPricingInputReader>();
        services.AddScoped<Catalog.Application.Abstractions.Pricing.ISewingPricingSettingsReader, Catalog.Infrastructure.Pricing.Reading.SewingPricingSettingsReader>();
        services.AddScoped<ISeeder, ProductCategorySeeder>();
        services.AddScoped<Catalog.Application.Abstractions.Pricing.ISewingPricingSettingsWriter, Catalog.Infrastructure.Pricing.SewingSettings.Writing.SewingPricingSettingsWriter>();
        services.AddScoped<Catalog.Application.Abstractions.Pricing.ISewingPricingTransaction, Catalog.Infrastructure.Pricing.History.Transactions.SewingPricingTransaction>();
        services.AddScoped<Catalog.Application.Abstractions.Pricing.ISewingCalculationStore, Catalog.Infrastructure.Pricing.History.Calculations.SewingCalculationStore>();
        services.AddScoped<Catalog.Application.Abstractions.Pricing.ISewingPublicationStore, Catalog.Infrastructure.Pricing.History.Publications.SewingPublicationStore>();
        services.AddScoped<Catalog.Application.Abstractions.Pricing.ISewingPublishedPriceReader, Catalog.Infrastructure.Pricing.History.Publications.Reading.SewingPublishedPriceReader>();

        services.AddScoped<Catalog.Application.Features.Pricing.Shared.Sewing.AutoPublication.SewingPriceStatusReader>();
        services.AddScoped<Catalog.Application.Features.Pricing.Shared.Sewing.AutoPublication.SewingPricePublisher>();
        services.AddScoped<Catalog.Application.Abstractions.IProductOverviewVariantReader, Catalog.Infrastructure.Products.Overview.ProductOverviewVariantReader>();
        services.AddScoped<Catalog.Application.Features.Product.Shared.Overview.Items.ProductOverviewItemReader>();

        services.AddScoped<Catalog.Application.Abstractions.Pricing.IPpePricingInputReader, Catalog.Infrastructure.Pricing.Ppe.Reading.PpePricingInputReader>();
        services.AddScoped<Catalog.Application.Abstractions.Pricing.IPpeRuleVersionWriter, Catalog.Infrastructure.Pricing.Ppe.Writing.PpeRuleVersionWriter>();
        services.AddScoped<Catalog.Application.Abstractions.Pricing.IPpeOfferVersionWriter, Catalog.Infrastructure.Pricing.Ppe.Writing.PpeOfferVersionWriter>();
        services.AddScoped<Catalog.Application.Abstractions.ISheetProductImportStore, Catalog.Infrastructure.Products.SheetImport.SheetProductImportStore>();
        services.AddScoped<Catalog.Application.Abstractions.Pricing.IPpePriceSourceStore, Catalog.Infrastructure.Pricing.Ppe.SupplierPrices.Store.PpePriceSourceStore>();
        services.AddScoped<Catalog.Application.Abstractions.Pricing.IPpeSupplierFileReader, Catalog.Infrastructure.Pricing.Ppe.SupplierPrices.Files.PpeSupplierFileReader>();
        services.AddHttpClient<Catalog.Infrastructure.Pricing.Ppe.SupplierPrices.Online.SupplierPageClient>(client => client.Timeout = TimeSpan.FromSeconds(30));
        services.Configure<Catalog.Infrastructure.Pricing.Ppe.SupplierPrices.Online.Cabinets.SupplierCabinetOptions>(
            configuration.GetSection(Catalog.Infrastructure.Pricing.Ppe.SupplierPrices.Online.Cabinets.SupplierCabinetOptions.Section));
        services.AddHttpClient<Catalog.Infrastructure.Pricing.Ppe.SupplierPrices.Online.Cabinets.SupplierCabinetClient>(client => client.Timeout = TimeSpan.FromSeconds(30))
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AllowAutoRedirect = false, UseCookies = false });
        services.AddScoped<Catalog.Infrastructure.Pricing.Ppe.SupplierPrices.Online.Cabinets.SupplierCabinetPriceReader>();
        services.AddScoped<Catalog.Application.Abstractions.Pricing.IPpeSupplierOnlinePriceReader, Catalog.Infrastructure.Pricing.Ppe.SupplierPrices.Online.PpeSupplierOnlinePriceReader>();
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
