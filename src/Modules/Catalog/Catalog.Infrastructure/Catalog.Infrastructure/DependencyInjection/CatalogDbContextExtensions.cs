using BuildingBlocks.Infrastructure.Migrations;
using BuildingBlocks.Infrastructure.Seeding;
using Catalog.Application.Contracts.Persistence;
using Catalog.Infrastructure.DataBase;
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

        services.AddDbContext<CatalogDbContext>(options => options.UseNpgsql(connectionString));

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

        services.AddScoped<Catalog.Application.Contracts.Pricing.Ppe.IPpePricingInputReader, Catalog.Infrastructure.Pricing.Ppe.Reading.PpePricingInputReader>();
        services.AddScoped<Catalog.Application.Contracts.Pricing.Ppe.IPpeRuleVersionWriter, Catalog.Infrastructure.Pricing.Ppe.Writing.PpeRuleVersionWriter>();
        services.AddScoped<Catalog.Application.Contracts.Pricing.Ppe.IPpeOfferVersionWriter, Catalog.Infrastructure.Pricing.Ppe.Writing.PpeOfferVersionWriter>();
        return services;
    }
}
