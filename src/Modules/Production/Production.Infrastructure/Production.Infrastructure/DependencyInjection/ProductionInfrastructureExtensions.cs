using BuildingBlocks.Infrastructure.Migrations;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Production.Application.Abstractions;
using Production.Application.Orders.QuoteSources;
using Production.Infrastructure.Persistence;

namespace Production.Infrastructure.DependencyInjection;

public static class ProductionInfrastructureExtensions
{
    /// <summary>DbContext and migrator of the sewing workshop module.</summary>
    public static IServiceCollection AddProductionInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connection = configuration.GetConnectionString("Default") ?? throw new InvalidOperationException("Missing ConnectionStrings:Default");
        services.AddDbContext<ProductionDbContext>(options => options.UseNpgsql(connection,
            x => x.MigrationsHistoryTable("__EFMigrationsHistory", ProductionDbContext.Schema)));
        services.AddScoped<IProductionDbContext>(sp => sp.GetRequiredService<ProductionDbContext>());
        services.AddScoped<IDatabaseMigrator, DbMigrator<ProductionDbContext>>();
        services.AddScoped<QuoteItemResolver>();
        services.TryAddSingleton(TimeProvider.System);
        return services;
    }
}
