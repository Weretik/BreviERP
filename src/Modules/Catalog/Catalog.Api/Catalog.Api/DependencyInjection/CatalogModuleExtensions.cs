using Catalog.Api.Endpoints.Overview;
using Catalog.Api.Endpoints.Photos;
using Catalog.Api.Endpoints.Ppe;
using Catalog.Api.Endpoints.Product;
using Catalog.Api.Endpoints.ProductCategory;
using Catalog.Api.Endpoints.Sewing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using BuildingBlocks.Application.DependencyInjection;
using Catalog.Application;
using Catalog.Contracts.Pricing;
using Catalog.Infrastructure.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Catalog.Api;
using Catalog.Api.Middleware;

namespace Catalog.Api.DependencyInjection;

public static class CatalogModuleExtensions
{
    /// <summary>Registers the Catalog module: Application, Infrastructure, and its Contracts implementations.</summary>
    public static IServiceCollection AddCatalogModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCatalogInfrastructure(configuration);
        // Publishes new Sewing prices after any request that saved a pricing input, in this or another module.
        services.TryAddScoped<PricingInputChanges>();
        services.AddTransient<IStartupFilter, SewingPriceAutoPublicationStartupFilter>();
        services.AddModuleValidators(typeof(CatalogApplicationAssemblyMarker).Assembly);
        return services;
    }

    /// <summary>Maps the Catalog module's HTTP endpoints.</summary>
    public static IEndpointRouteBuilder MapCatalogEndpoints(this IEndpointRouteBuilder app) =>
        app.MapProductOverviewEndpoints().MapOldSitePhotoImportEndpoints().MapProductPhotosEndpoints()
            .MapPpePricingRulesEndpoints().MapPpeProductPricingEndpoints().MapPpeSupplierOfferEndpoints()
            .MapPpeSupplierPricesEndpoints().MapProductsEndpoints().MapProductCategoriesEndpoints()
            .MapSewingArticleSummaryEndpoints().MapSewingArticlesEndpoints().MapSewingCompositionEndpoints()
            .MapSewingPricingEndpoints().MapSewingPricingSettingsEndpoints().MapSewingPublishedProductsEndpoints();
}
