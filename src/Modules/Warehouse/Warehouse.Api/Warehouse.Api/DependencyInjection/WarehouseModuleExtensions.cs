using BuildingBlocks.Application.DependencyInjection;
using Warehouse.Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Warehouse.Infrastructure.DependencyInjection;

namespace Warehouse.Api.DependencyInjection;

public static class WarehouseModuleExtensions
{
    /// <summary>Registers the Warehouse module: Application, Infrastructure, its Contracts implementations and controllers.</summary>
    public static IServiceCollection AddWarehouseModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddWarehouseInfrastructure(configuration);
        services.AddModuleValidators(typeof(WarehouseApplicationAssemblyMarker).Assembly);
        services.AddControllers().AddApplicationPart(typeof(WarehouseApiAssemblyMarker).Assembly);
        return services;
    }
}
