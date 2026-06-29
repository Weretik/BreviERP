using Catalog.Application.Contracts.Storage;
using Catalog.Infrastructure.Services;

namespace Catalog.Infrastructure.DependencyInjection;

public static class CatalogInfrastructureExtensions
{
    public static IServiceCollection AddCatalogInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddCatalogDbContextServices(configuration);
        services.AddAdmToolsHttpClient(configuration);
        services.AddSingleton<IMediaStorageService, AdmToolsMediaStorageService>();

        return services;
    }
}
