using Reference.Api.Endpoints.FabricColors;
using Reference.Api.Endpoints.Fabrics;
using Reference.Api.Endpoints.GarmentAccessories;
using Reference.Api.Endpoints.GarmentPartOperations;
using Reference.Api.Endpoints.GarmentParts;
using Reference.Api.Endpoints.OtherCostPricing;
using Reference.Api.Endpoints.PpeMarkupRules;
using Reference.Api.Endpoints.PricingParameters;
using Reference.Api.Endpoints.ProductSections;
using Reference.Api.Endpoints.Suppliers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Reference.Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Reference.Infrastructure.DependencyInjection;
using Reference.Api;
using BuildingBlocks.Application.Validation;

namespace Reference.Api.DependencyInjection;

public static class ReferenceModuleExtensions
{
    /// <summary>Registers the Reference module: Application, Infrastructure, and its Contracts implementations.</summary>
    public static IServiceCollection AddReferenceModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddReferenceInfrastructure(configuration);
        services.AddModuleValidators(typeof(ReferenceApplicationAssemblyMarker).Assembly);
        return services;
    }

    /// <summary>Maps the Reference module's HTTP endpoints.</summary>
    public static IEndpointRouteBuilder MapReferenceEndpoints(this IEndpointRouteBuilder app) =>
        app.MapFabricsEndpoints().MapFabricColorsEndpoints().MapGarmentAccessoriesEndpoints().MapGarmentPartsEndpoints()
            .MapGarmentPartOperationsEndpoints().MapProductSectionsEndpoints().MapPricingParametersEndpoints().MapPpeMarkupRulesEndpoints()
            .MapOtherCostPricingEndpoints().MapSuppliersEndpoints();
}
