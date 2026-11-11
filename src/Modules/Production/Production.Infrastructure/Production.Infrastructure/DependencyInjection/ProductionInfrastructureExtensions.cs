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
        services.AddScoped<QuoteItemResolver>();
        services.AddScoped<OrderCards>();
        services.AddScoped<Production.Application.Orders.Embroidery.EmbroideryLinks>();
        services.AddScoped<OrderTracking>();
        services.AddScoped<Production.Application.Colors.ProductionColors>();
        services.AddScoped<WorkCards>();
        services.AddScoped<Production.Application.FabricPlans.ProductionFabricPlans>();
        services.AddScoped<ForecastQueueReader>();
        services.AddScoped<PayrollSheets>();
        services.AddScoped<WorkshopReports>();
        services.AddScoped<PayrollChanges>();
        services.AddScoped<TimesheetDays>();
        services.TryAddSingleton(new ProductionLinks(configuration["Production:AdminUrl"] is { Length: > 0 } url
            ? url
            : "https://office.brevi.com.ua"));
        services.TryAddSingleton(TimeProvider.System);
        return services;
    }
}
