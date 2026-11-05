using BuildingBlocks.Application.Pricing;
using Accounting.Api;
using Catalog.Api;
using Crm.Api;
using Identity.Api;
using Reference.Api;
using Reminders.Api;
using Media.Api;
using Production.Api;
using Purchasing.Api;
using Host.Api.Pricing;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Host.Api.DependencyInjection.ServiceRegistration.Web;

public static class ControllersExtensions
{
    public static IMvcBuilder AddModuleControllers(this IServiceCollection services)
    {
        // The modules' DbContext registrations add the same scoped instance; a host without them gets an empty one.
        services.TryAddScoped<PricingInputChanges>();
        return services.AddControllers(options => options.Filters.Add<SewingPriceAutoPublicationFilter>())
            .AddApplicationPart(typeof(AccountingApiAssemblyMarker).Assembly)
            .AddApplicationPart(typeof(CatalogApiAssemblyMarker).Assembly)
            .AddApplicationPart(typeof(CrmApiAssemblyMarker).Assembly)
            .AddApplicationPart(typeof(IdentityApiAssemblyMarker).Assembly)
            .AddApplicationPart(typeof(ReferenceApiAssemblyMarker).Assembly)
            .AddApplicationPart(typeof(RemindersApiAssemblyMarker).Assembly)
            .AddApplicationPart(typeof(MediaApiAssemblyMarker).Assembly)
            .AddApplicationPart(typeof(ProductionApiAssemblyMarker).Assembly)
            .AddApplicationPart(typeof(PurchasingApiAssemblyMarker).Assembly);
    }
}
