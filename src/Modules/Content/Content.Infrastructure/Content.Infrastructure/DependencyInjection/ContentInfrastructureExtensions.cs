using Content.Infrastructure.Adapters.Catalog;
using Content.Infrastructure.Adapters.Media;
using BuildingBlocks.Infrastructure.Migrations;
using Content.Application.Abstractions;
using Content.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Content.Application.Features.Articles.Shared;
using Content.Application.Features.Catalog.Shared;
using Content.Application.Features.Redirects.Shared;
using Content.Application.Features.SiteCompany.Shared;
using Content.Application.Features.SiteGalleries.Shared;
using Content.Application.Features.SitePages.Shared;
using Content.Application.Features.SiteSettings.Shared;

namespace Content.Infrastructure.DependencyInjection;

public static class ContentInfrastructureExtensions
{
    /// <summary>DbContext of schema <c>content</c>, its migrator and the short-lived sources of the public catalog, articles, redirects and site settings.</summary>
    public static IServiceCollection AddContentInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connection = configuration.GetConnectionString("Default") ?? throw new InvalidOperationException("Missing ConnectionStrings:Default");
        services.AddDbContext<ContentDbContext>(options => options.UseNpgsql(connection,
            x => x.MigrationsHistoryTable("__EFMigrationsHistory", ContentDbContext.Schema)));
        services.AddScoped<IContentDbContext>(sp => sp.GetRequiredService<ContentDbContext>());
        services.AddScoped<IDatabaseMigrator, DbMigrator<ContentDbContext>>();
        services.AddMemoryCache();
        services.AddScoped<IPublicCatalogSource, PublicCatalogSource>();
        services.AddScoped<IPublicArticlesSource, PublicArticlesSource>();
        services.AddScoped<IPublicRedirectsSource, PublicRedirectsSource>();
        services.AddScoped<IPublicSiteSettingsSource, PublicSiteSettingsSource>();
        services.AddScoped<IPublicSitePagesSource, PublicSitePagesSource>();
        services.AddScoped<IPublicSiteCompanySource, PublicSiteCompanySource>();
        services.AddScoped<IPublicSiteGalleriesSource, PublicSiteGalleriesSource>();
        services.TryAddSingleton(TimeProvider.System);

        // Adapters from other modules' Contracts to this module's ports.
        services.AddScoped<IContentMediaReader, ContentMediaReader>();
        services.AddScoped<IContentPhotoImporter, ContentPhotoImporter>();
        services.AddScoped<IContentCatalogReader, ContentCatalogReader>();
        return services;
    }
}
