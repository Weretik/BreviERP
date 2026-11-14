using BuildingBlocks.Infrastructure.Outbox;
using Production.Application.Orders.Telegram;
using Production.Infrastructure.Adapters.Catalog;
using Production.Infrastructure.Adapters.Crm;
using Production.Infrastructure.Adapters.Identity;
using Production.Infrastructure.Contracts.Orders;
using Production.Contracts.Orders;
using Production.Infrastructure.Adapters.Media;
using Production.Infrastructure.Adapters.Reference;
using Production.Infrastructure.Adapters.Reminders;
using Production.Infrastructure.Adapters.Warehouse;
using BuildingBlocks.Infrastructure.Migrations;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Production.Application.Abstractions;
using Production.Application.Orders;
using Production.Application.Orders.QuoteSources;
using Production.Application.Orders.Tracking;
using Production.Application.Work;
using Production.Application.Forecast;
using Production.Application.Payroll;
using Production.Application.Reports;
using Production.Application.Timesheet;
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
        services.AddOutbox<ProductionDbContext>();
        services.AddOutboxHandler<SendProductionOrderTelegramOutboxHandler>();
        services.AddScoped<QuoteItemResolver>();
        services.AddScoped<OrderCards>();
        services.AddScoped<Production.Application.Orders.Embroidery.EmbroideryLinks>();
        services.AddScoped<OrderTracking>();
        services.AddScoped<Production.Application.Colors.ProductionColors>();
        services.AddScoped<WorkCards>();
        services.AddScoped<Production.Application.FabricPlans.ProductionFabricPlans>();
        services.AddScoped<Production.Application.FabricPlans.ProductionAssemblies>();
        services.AddScoped<Production.Contracts.FabricPlans.IOrderFabricPlans>(sp =>
            sp.GetRequiredService<Production.Application.FabricPlans.ProductionFabricPlans>());
        services.AddScoped<ForecastQueueReader>();
        services.AddScoped<PayrollSheets>();
        services.AddScoped<WorkshopReports>();
        services.AddScoped<PayrollChanges>();
        services.AddScoped<TimesheetDays>();
        services.TryAddSingleton(new ProductionLinks(configuration["Production:AdminUrl"] is { Length: > 0 } url
            ? url
            : "https://office.brevi.com.ua"));
        services.TryAddSingleton(TimeProvider.System);

        // Adapters from other modules' Contracts to this module's ports.
        services.AddScoped<IProductionFabricReader, ProductionFabricReader>();
        services.AddScoped<IProductionColorReader, ProductionColorReader>();
        services.AddScoped<IProductionStockReader, ProductionStockReader>();
        services.AddScoped<IProductionStaffReader, ProductionStaffReader>();
        services.AddScoped<IPayrollParameterReader, PayrollParameterReader>();
        services.AddScoped<IProductionFileStorage, ProductionFileStorage>();
        services.AddScoped<IProductionTelegram, ProductionTelegram>();
        services.AddScoped<IProductionModelReader, ProductionModelReader>();
        services.AddScoped<IProductionQuoteReader, ProductionQuoteReader>();
        services.AddScoped<IProductionEmbroideryReader, ProductionEmbroideryReader>();

        // Production.Contracts for other modules.
        services.AddScoped<IQuoteOrderLinks, QuoteOrderLinks>();
        return services;
    }
}
