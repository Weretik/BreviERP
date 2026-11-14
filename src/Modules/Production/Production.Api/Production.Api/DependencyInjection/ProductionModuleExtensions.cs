using Production.Api.Endpoints.Archive;
using Production.Api.Endpoints.Dashboard;
using Production.Api.Endpoints.FabricNorms;
using Production.Api.Endpoints.Forecast;
using Production.Api.Endpoints.Orders;
using Production.Api.Endpoints.Payroll;
using Production.Api.Endpoints.Reports;
using Production.Api.Endpoints.Settings;
using Production.Api.Endpoints.Staff;
using Production.Api.Endpoints.Timesheet;
using Production.Api.Endpoints.Work;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Production.Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Production.Infrastructure.DependencyInjection;
using BuildingBlocks.Application.Validation;

namespace Production.Api.DependencyInjection;

public static class ProductionModuleExtensions
{
    /// <summary>Registers the Production module: Application, Infrastructure, and its Contracts implementations.</summary>
    public static IServiceCollection AddProductionModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddProductionInfrastructure(configuration);
        services.AddModuleValidators(typeof(ProductionApplicationAssemblyMarker).Assembly);
        return services;
    }

    /// <summary>Maps the Production module's HTTP endpoints.</summary>
    public static IEndpointRouteBuilder MapProductionEndpoints(this IEndpointRouteBuilder app) =>
        app.MapWorkshopArchiveEndpoints().MapWorkshopDashboardEndpoints().MapFabricNormsEndpoints()
            .MapProductionForecastEndpoints().MapProductionOrderTrackingEndpoints().MapProductionOrdersEndpoints()
            .MapPayrollEndpoints().MapWorkshopReportsEndpoints().MapWorkshopSettingsEndpoints()
            .MapWorkshopStaffEndpoints().MapTimesheetEndpoints().MapProductionWorkEndpoints();
}
