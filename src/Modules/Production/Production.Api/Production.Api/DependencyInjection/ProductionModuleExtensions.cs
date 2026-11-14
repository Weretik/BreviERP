using BuildingBlocks.Application.DependencyInjection;
using Production.Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Production.Infrastructure.DependencyInjection;

namespace Production.Api.DependencyInjection;

public static class ProductionModuleExtensions
{
    /// <summary>Registers the Production module: Application, Infrastructure, its Contracts implementations and controllers.</summary>
    public static IServiceCollection AddProductionModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddProductionInfrastructure(configuration);
        services.AddModuleValidators(typeof(ProductionApplicationAssemblyMarker).Assembly);
        services.AddControllers().AddApplicationPart(typeof(ProductionApiAssemblyMarker).Assembly);
        return services;
    }
}
