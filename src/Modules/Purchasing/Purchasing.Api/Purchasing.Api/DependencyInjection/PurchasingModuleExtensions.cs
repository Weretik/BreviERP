using BuildingBlocks.Application.DependencyInjection;
using Purchasing.Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Purchasing.Api.Endpoints.Documents;
using Purchasing.Api.Endpoints.Register;
using Purchasing.Infrastructure.DependencyInjection;

namespace Purchasing.Api.DependencyInjection;

public static class PurchasingModuleExtensions
{
    /// <summary>Registers the Purchasing module: Application, Infrastructure, and its Contracts implementations.</summary>
    public static IServiceCollection AddPurchasingModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPurchasingInfrastructure(configuration);
        services.AddModuleValidators(typeof(PurchasingApplicationAssemblyMarker).Assembly);
        return services;
    }

    /// <summary>Maps the Purchasing module's HTTP endpoints.</summary>
    public static IEndpointRouteBuilder MapPurchasingEndpoints(this IEndpointRouteBuilder app) =>
        app.MapRegisterEndpoints().MapDocumentsEndpoints();
}
