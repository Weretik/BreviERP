using Content.Api.Endpoints.Articles;
using Content.Api.Endpoints.ProductPages;
using Content.Api.Endpoints.PublicArticles;
using Content.Api.Endpoints.PublicCatalog;
using Content.Api.Endpoints.PublicRedirects;
using Content.Api.Endpoints.PublicSiteCompany;
using Content.Api.Endpoints.PublicSiteGalleries;
using Content.Api.Endpoints.PublicSitePages;
using Content.Api.Endpoints.PublicSiteSettings;
using Content.Api.Endpoints.Redirects;
using Content.Api.Endpoints.SeoOverview;
using Content.Api.Endpoints.SiteCategories;
using Content.Api.Endpoints.SiteCompany;
using Content.Api.Endpoints.SiteGalleries;
using Content.Api.Endpoints.SitePages;
using Content.Api.Endpoints.SiteSettings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using BuildingBlocks.Application.DependencyInjection;
using Content.Application;
using Content.Infrastructure.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Content.Api.DependencyInjection;

public static class ContentModuleExtensions
{
    /// <summary>Registers the Content module: Application, Infrastructure, and its Contracts implementations.</summary>
    public static IServiceCollection AddContentModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddContentInfrastructure(configuration);
        services.AddModuleValidators(typeof(ContentApplicationAssemblyMarker).Assembly);
        return services;
    }

    /// <summary>Maps the Content module's HTTP endpoints.</summary>
    public static IEndpointRouteBuilder MapContentEndpoints(this IEndpointRouteBuilder app) =>
        app.MapContentArticlesEndpoints().MapContentProductsEndpoints().MapPublicArticlesEndpoints()
            .MapPublicCatalogEndpoints().MapPublicRedirectsEndpoints().MapPublicSiteCompanyEndpoints()
            .MapPublicSiteGalleriesEndpoints().MapPublicSitePagesEndpoints().MapPublicSiteSettingsEndpoints()
            .MapContentRedirectsEndpoints().MapSeoOverviewEndpoints().MapSiteCategoriesEndpoints()
            .MapSiteCompanyEndpoints().MapSiteCertificatesEndpoints().MapSiteWorksEndpoints().MapSitePagesEndpoints()
            .MapSiteSeoSettingsEndpoints();
}
