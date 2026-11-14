using Warehouse.Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Warehouse.Api.Endpoints.FabricStock;
using Warehouse.Api.Endpoints.OrderItemFabric;
using Warehouse.Api.Endpoints.Reports;
using Warehouse.Infrastructure.DependencyInjection;
using BuildingBlocks.Application.Validation;

namespace Warehouse.Api.DependencyInjection;

public static class WarehouseModuleExtensions
{
    /// <summary>Registers the Warehouse module: Application, Infrastructure, and its Contracts implementations.</summary>
    public static IServiceCollection AddWarehouseModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddWarehouseInfrastructure(configuration);
        services.AddModuleValidators(typeof(WarehouseApplicationAssemblyMarker).Assembly);
        return services;
    }

    /// <summary>Maps the Warehouse module's HTTP endpoints.</summary>
    public static IEndpointRouteBuilder MapWarehouseEndpoints(this IEndpointRouteBuilder app) =>
        app.MapFabricStockEndpoints().MapOrderItemFabricEndpoints().MapReportsEndpoints();
}
