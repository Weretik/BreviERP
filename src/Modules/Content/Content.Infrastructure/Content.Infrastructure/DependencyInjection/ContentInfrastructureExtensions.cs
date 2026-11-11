using BuildingBlocks.Infrastructure.Migrations;
using Content.Application.Abstractions;
using Content.Application.PublicCatalog;
using Content.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Content.Infrastructure.DependencyInjection;

public static class ContentInfrastructureExtensions
{
    /// <summary>DbContext of schema <c>content</c>, its migrator and the short-lived source of the public catalog.</summary>
    public static IServiceCollection AddContentInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connection = configuration.GetConnectionString("Default") ?? throw new InvalidOperationException("Missing ConnectionStrings:Default");
        services.AddDbContext<ContentDbContext>(options => options.UseNpgsql(connection,
            x => x.MigrationsHistoryTable("__EFMigrationsHistory", ContentDbContext.Schema)));
        services.AddScoped<IContentDbContext>(sp => sp.GetRequiredService<ContentDbContext>());
        services.AddScoped<IDatabaseMigrator, DbMigrator<ContentDbContext>>();
        services.AddMemoryCache();
        services.AddScoped<IPublicCatalogSource, PublicCatalogSource>();
        services.TryAddSingleton(TimeProvider.System);
        return services;
    }
}
