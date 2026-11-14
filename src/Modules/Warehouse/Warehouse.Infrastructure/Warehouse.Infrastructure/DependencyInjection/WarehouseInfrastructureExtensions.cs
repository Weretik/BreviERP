using Warehouse.Infrastructure.Adapters.Identity;
using Warehouse.Infrastructure.Adapters.Production;
using Warehouse.Infrastructure.Adapters.Reference;
using Warehouse.Contracts.Stock;
using BuildingBlocks.Infrastructure.Migrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Warehouse.Application.Abstractions;
using Warehouse.Application.Colors;
using Warehouse.Application.Stock;
using Warehouse.Application.Stock.Facts;
using Warehouse.Application.Stock.Issues;
using Warehouse.Application.Stock.Reports;
using Warehouse.Application.Stock.Revisions;
using Warehouse.Infrastructure.Persistence;

namespace Warehouse.Infrastructure.DependencyInjection;

public static class WarehouseInfrastructureExtensions
{
    /// <summary>
    /// DbContext of schema <c>warehouse</c>, its migrator and the stock services. Register it before Production: its
    /// first migration copies «Склад тканин» from the production schema, which Production then drops.
    /// </summary>
    public static IServiceCollection AddWarehouseInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connection = configuration.GetConnectionString("Default") ?? throw new InvalidOperationException("Missing ConnectionStrings:Default");
        services.AddDbContext<WarehouseDbContext>(options => options.UseNpgsql(connection,
            x => x.MigrationsHistoryTable("__EFMigrationsHistory", WarehouseDbContext.Schema)));
        services.AddScoped<IWarehouseDbContext>(sp => sp.GetRequiredService<WarehouseDbContext>());
        services.AddScoped<IDatabaseMigrator, DbMigrator<WarehouseDbContext>>();
        services.AddScoped<StockReservations>();
        services.AddScoped<FabricBookingCalculator>();
        services.AddScoped<StockOperations>();
        services.AddScoped<StockPairs>();
        services.AddScoped<MovementRevisions>();
        services.AddScoped<StockChanges>();
        services.AddScoped<WarehouseColors>();
        services.AddScoped<ItemFabricCards>();
        services.AddScoped<ItemPlans>();
        services.AddScoped<StockFacts>();

        // Contracts other modules use (ADR-0006).
        services.AddScoped<IStockFacts>(sp => sp.GetRequiredService<StockFacts>());
        services.TryAddSingleton(TimeProvider.System);

        // Adapters from other modules' Contracts to this module's ports.
        services.AddScoped<IWarehouseReferenceReader, WarehouseReferenceReader>();
        services.AddScoped<IWarehouseStaffReader, WarehouseStaffReader>();
        services.AddScoped<IWarehouseProductionReader, WarehouseProductionReader>();
        return services;
    }
}
