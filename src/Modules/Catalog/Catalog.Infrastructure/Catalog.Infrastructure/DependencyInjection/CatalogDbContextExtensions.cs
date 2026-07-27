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
        services.AddScoped<IDatabaseMigrator, DbMigrator<CatalogDbContext>>();
        services.AddScoped<ISeeder, ProductCategorySeeder>();

        return services;
    }
}