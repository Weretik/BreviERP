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
        services.AddScoped<ISeeder, SupplierSeeder>();
        services.AddScoped<ISeeder, FabricSeeder>();
        services.AddScoped<ISeeder, GarmentAccessorySeeder>();
        services.AddScoped<ISeeder, GarmentPartSeeder>();
        services.AddScoped<ISeeder, GarmentPartOperationSeeder>();

        return services;
    }
}
