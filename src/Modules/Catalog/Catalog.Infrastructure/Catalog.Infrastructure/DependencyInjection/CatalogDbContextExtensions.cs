using BuildingBlocks.Application.Pricing;
using Microsoft.Extensions.DependencyInjection.Extensions;
using BuildingBlocks.Infrastructure.Migrations;
using BuildingBlocks.Infrastructure.Seeding;
using Catalog.Application.Contracts.Persistence;
using Catalog.Infrastructure.DataBase;
using Catalog.Infrastructure.Pricing.AutoPublication;
using Catalog.Infrastructure.Repositories;
using Catalog.Infrastructure.Seeders.ProductCategories;

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
        services.AddScoped<Catalog.Application.Features.Product.Sewing.ISewingCompositionStore, SewingCompositionStore>();
        services.AddScoped<Catalog.Application.Features.Product.Sewing.ISewingCompositionReadStore, SewingCompositionReadStore>();
        services.AddScoped<IDatabaseMigrator, DbMigrator<CatalogDbContext>>();
        services.AddScoped<Catalog.Application.Contracts.Pricing.ISewingPricingInputReader, Catalog.Infrastructure.Pricing.Reading.SewingPricingInputReader>();
        services.AddScoped<Catalog.Application.Contracts.Pricing.ISewingPricingSettingsReader, Catalog.Infrastructure.Pricing.Reading.SewingPricingSettingsReader>();
        services.AddScoped<ISeeder, ProductCategorySeeder>();
        services.AddScoped<Catalog.Application.Contracts.Pricing.ISewingPricingSettingsWriter, Catalog.Infrastructure.Pricing.SewingSettings.Writing.SewingPricingSettingsWriter>();
        services.AddScoped<Catalog.Application.Contracts.Pricing.ISewingPricingTransaction, Catalog.Infrastructure.Pricing.History.Transactions.SewingPricingTransaction>();
        services.AddScoped<Catalog.Application.Contracts.Pricing.ISewingCalculationStore, Catalog.Infrastructure.Pricing.History.Calculations.SewingCalculationStore>();
        services.AddScoped<Catalog.Application.Contracts.Pricing.ISewingPublicationStore, Catalog.Infrastructure.Pricing.History.Publications.SewingPublicationStore>();
        services.AddScoped<Catalog.Application.Contracts.Pricing.ISewingPublishedPriceReader, Catalog.Infrastructure.Pricing.History.Publications.Reading.SewingPublishedPriceReader>();

        services.AddScoped<Catalog.Application.Features.Pricing.Sewing.AutoPublication.SewingPriceStatusReader>();
        services.AddScoped<Catalog.Application.Features.Pricing.Sewing.AutoPublication.SewingPricePublisher>();
        services.AddScoped<Catalog.Application.Contracts.Persistence.ProductOverview.IProductOverviewVariantReader, Catalog.Infrastructure.Products.Overview.ProductOverviewVariantReader>();
        services.AddScoped<Catalog.Application.Features.Product.Overview.Shared.Items.ProductOverviewItemReader>();

        services.AddScoped<Catalog.Application.Contracts.Pricing.Ppe.IPpePricingInputReader, Catalog.Infrastructure.Pricing.Ppe.Reading.PpePricingInputReader>();
        services.AddScoped<Catalog.Application.Contracts.Pricing.Ppe.IPpeRuleVersionWriter, Catalog.Infrastructure.Pricing.Ppe.Writing.PpeRuleVersionWriter>();
        services.AddScoped<Catalog.Application.Contracts.Pricing.Ppe.IPpeOfferVersionWriter, Catalog.Infrastructure.Pricing.Ppe.Writing.PpeOfferVersionWriter>();
        services.AddScoped<Catalog.Application.Contracts.Persistence.SheetImport.ISheetProductImportStore, Catalog.Infrastructure.Products.SheetImport.SheetProductImportStore>();
        services.AddScoped<Catalog.Application.Contracts.Pricing.Ppe.SupplierPrices.IPpePriceSourceStore, Catalog.Infrastructure.Pricing.Ppe.SupplierPrices.Store.PpePriceSourceStore>();
        services.AddScoped<Catalog.Application.Contracts.Pricing.Ppe.SupplierPrices.IPpeSupplierFileReader, Catalog.Infrastructure.Pricing.Ppe.SupplierPrices.Files.PpeSupplierFileReader>();
        services.AddHttpClient<Catalog.Infrastructure.Pricing.Ppe.SupplierPrices.Online.SupplierPageClient>(client => client.Timeout = TimeSpan.FromSeconds(30));
        services.AddScoped<Catalog.Application.Contracts.Pricing.Ppe.SupplierPrices.IPpeSupplierOnlinePriceReader, Catalog.Infrastructure.Pricing.Ppe.SupplierPrices.Online.PpeSupplierOnlinePriceReader>();
        services.AddScoped<Catalog.Application.Features.Pricing.Ppe.SupplierPrices.Running.PpePriceImportRunner>();
        services.AddScoped<Catalog.Application.Features.Pricing.Ppe.SupplierPrices.Refresh.PpeScheduledPriceRefresh>();
        return services;
    }
}
