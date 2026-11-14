using BuildingBlocks.Application.DependencyInjection;
using Catalog.Application;
using Catalog.Contracts.Pricing;
using Catalog.Api.Pricing;
using Catalog.Infrastructure.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Catalog.Api.DependencyInjection;

public static class CatalogModuleExtensions
{
    /// <summary>Registers the Catalog module: Application, Infrastructure, its Contracts implementations and controllers.</summary>
    public static IServiceCollection AddCatalogModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCatalogInfrastructureServices(configuration);
        // Publishes new Sewing prices after any action that saved a pricing input, in this or another module.
        services.TryAddScoped<PricingInputChanges>();
        services.Configure<MvcOptions>(options => options.Filters.Add<SewingPriceAutoPublicationFilter>());
        services.AddModuleValidators(typeof(CatalogApplicationAssemblyMarker).Assembly);
        services.AddControllers().AddApplicationPart(typeof(CatalogApiAssemblyMarker).Assembly);
        return services;
    }
}
