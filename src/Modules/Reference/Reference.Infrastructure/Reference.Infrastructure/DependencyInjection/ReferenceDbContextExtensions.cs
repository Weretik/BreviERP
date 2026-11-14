using Reference.Application.Contracts.Catalog;
using Reference.Infrastructure.Adapters.Catalog;
using Reference.Contracts.Fabrics;
using Reference.Infrastructure.Contracts.Fabrics;
using Reference.Contracts.GarmentAccessories;
using Reference.Infrastructure.Contracts.GarmentAccessories;
using Reference.Contracts.GarmentPartOperations;
using Reference.Infrastructure.Contracts.GarmentPartOperations;
using Reference.Contracts.Suppliers;
using Reference.Infrastructure.Contracts.Suppliers;
using Reference.Contracts.FabricColors;
using Reference.Infrastructure.Contracts.FabricColors;
using Reference.Contracts.PricingParameters;
using Reference.Infrastructure.Contracts.PricingParameters;
using Reference.Contracts.PpeMarkupRules;
using Reference.Infrastructure.Contracts.PpeMarkupRules;
using Reference.Contracts.ProductSections;
using Reference.Infrastructure.Contracts.ProductSections;
using Reference.Contracts.Pricing;
using Reference.Infrastructure.Contracts.Pricing;
using BuildingBlocks.Application.Pricing;
using Microsoft.Extensions.DependencyInjection.Extensions;
using BuildingBlocks.Infrastructure.Migrations;
using BuildingBlocks.Infrastructure.Seeding;
using Reference.Application.Contracts.Persistence;
using Reference.Infrastructure.DataBase;
using Reference.Infrastructure.Pricing.InputVersions.Changes;
using Reference.Infrastructure.Repositories;
using Reference.Infrastructure.Seeders.Fabrics;
using Reference.Infrastructure.Seeders.GarmentAccessories;
using Reference.Infrastructure.Seeders.GarmentParts;
using Reference.Infrastructure.Seeders.GarmentPartOperations;
using Reference.Infrastructure.Seeders.Suppliers;

namespace Reference.Infrastructure.DependencyInjection;

public static class ReferenceDbContextExtensions
{
    public static IServiceCollection AddReferenceDbContextServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
                               ?? throw new InvalidOperationException("Missing ConnectionStrings:Default"); ;

        services.TryAddScoped<PricingInputChanges>();
        services.AddScoped<ReferencePricingInputChangeInterceptor>();
        services.AddDbContext<ReferenceDbContext>((provider, options) => options.UseNpgsql(connectionString)
            .AddInterceptors(provider.GetRequiredService<ReferencePricingInputChangeInterceptor>()));
        services.AddScoped<IReadReferenceDbContext>(sp => sp.GetRequiredService<ReferenceDbContext>());

        services.AddScoped(typeof(IReferenceRepository<>), typeof(ReferenceEfRepository<>));
        services.AddScoped(typeof(IReferenceReadRepository<>), typeof(ReferenceReadEfRepository<>));

        services.AddScoped<IDatabaseMigrator, DbMigrator<ReferenceDbContext>>();
        services.AddScoped<Reference.Application.Contracts.Pricing.IReferencePricingVersionReader, Reference.Infrastructure.Pricing.Reading.ReferencePricingVersionReader>();
        services.AddScoped<Reference.Application.Contracts.Pricing.IReferencePricingUsageReader, Reference.Infrastructure.Pricing.Reading.ReferencePricingUsageReader>();
        services.AddScoped<Reference.Application.Contracts.Pricing.IOtherCostPricingVersionWriter, Reference.Infrastructure.Pricing.OtherCosts.Writing.OtherCostPricingVersionWriter>();
        // Contracts other modules use (ADR-0006).
        services.AddScoped<IFabricDirectory, FabricDirectory>();
        services.AddScoped<IGarmentAccessoryDirectory, GarmentAccessoryDirectory>();
        services.AddScoped<IGarmentPartOperationDirectory, GarmentPartOperationDirectory>();
        services.AddScoped<ISupplierDirectory, SupplierDirectory>();
        services.AddScoped<IFabricColorDirectory, FabricColorDirectory>();
        services.AddScoped<IPricingParameterDirectory, PricingParameterDirectory>();
        services.AddScoped<IPpeMarkupRuleDirectory, PpeMarkupRuleDirectory>();
        services.AddScoped<IProductSectionDirectory, ProductSectionDirectory>();
        services.AddScoped<IPricingInputVersionReader, PricingInputVersionReader>();

        // Adapters from other modules' Contracts to this module's ports.
        services.AddScoped<IProductUsageReader, ProductUsageReader>();
        services.AddScoped<IPricingSettingsChangeNotifier, PricingSettingsChangeNotifier>();
        services.AddScoped<ISeeder, SupplierSeeder>();
        services.AddScoped<ISeeder, FabricSeeder>();
        services.AddScoped<ISeeder, GarmentAccessorySeeder>();
        services.AddScoped<ISeeder, GarmentPartSeeder>();
        services.AddScoped<ISeeder, GarmentPartOperationSeeder>();

        return services;
    }
}
