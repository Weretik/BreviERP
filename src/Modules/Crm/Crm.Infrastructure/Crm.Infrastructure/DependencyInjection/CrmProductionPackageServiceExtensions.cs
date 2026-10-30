using Crm.Application.CustomSewing.Production.Export;
using Crm.Application.CustomSewing.Production.Persistence;
using Crm.Application.CustomSewing.Production.Reading;
using Crm.Infrastructure.Documents.Production;
using Crm.Infrastructure.Persistence.Documents.Production;
using Microsoft.Extensions.DependencyInjection;

namespace Crm.Infrastructure.DependencyInjection;

public static class CrmProductionPackageServiceExtensions
{
    public static IServiceCollection AddCrmProductionPackages(this IServiceCollection services)
    {
        services.AddScoped<IProductionPackageStore, ProductionPackageStore>();
        services.AddScoped<IProductionPackageArchiveBuilder, ProductionPackageArchiveBuilder>();
        services.AddScoped<ProductionPackageExporter>();
        services.AddScoped<ProductionPackageReader>();
        return services;
    }
}
