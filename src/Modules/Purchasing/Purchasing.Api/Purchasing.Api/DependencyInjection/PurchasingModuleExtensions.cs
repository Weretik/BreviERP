using BuildingBlocks.Application.DependencyInjection;
using Purchasing.Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Purchasing.Infrastructure.DependencyInjection;

namespace Purchasing.Api.DependencyInjection;

public static class PurchasingModuleExtensions
{
    /// <summary>Registers the Purchasing module: Application, Infrastructure, its Contracts implementations and controllers.</summary>
    public static IServiceCollection AddPurchasingModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPurchasingInfrastructure(configuration);
        services.AddModuleValidators(typeof(PurchasingApplicationAssemblyMarker).Assembly);
        services.AddControllers().AddApplicationPart(typeof(PurchasingApiAssemblyMarker).Assembly);
        return services;
    }
}
