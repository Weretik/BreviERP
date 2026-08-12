using Accounting.Infrastructure.DependencyInjection;
using Catalog.Api.DependencyInjection;
using Catalog.Application.Contracts.Reference;
using Catalog.Application.Contracts.SlugGeneration;
using Catalog.Infrastructure.Products.SlugGeneration;
using Reference.Application.Contracts.Catalog;
using Catalog.Infrastructure.DependencyInjection;
using Host.Api.DependencyInjection.ServiceRegistration.Options;
using Identity.Infrastructure.DependencyInjection;
using Reference.Infrastructure.DependencyInjection;

namespace Host.Api.DependencyInjection.ServiceRegistration;

public static class ModuleRegistrationsExtensions
{
    public static IServiceCollection AddModuleRegistrations(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddIdentityConfiguration(configuration);
        services.AddIdentityInfrastructureServices(configuration);

        services.AddInfrastructureServices(configuration);

        services.AddReferenceInfrastructureServices(configuration);
        services.AddAccountingInfrastructureServices(configuration);
        services.AddCatalogInfrastructureServices(configuration);
        services.AddCatalogApiOptions(configuration);
        services.AddScoped<IProductReferenceReader, CatalogProductReferenceReader>();
        services.AddScoped<IProductUsageReader, CatalogProductUsageReader>();
        services.AddSingleton<IProductSlugGenerator, ProductSlugGenerator>();

        return services;
    }
}
