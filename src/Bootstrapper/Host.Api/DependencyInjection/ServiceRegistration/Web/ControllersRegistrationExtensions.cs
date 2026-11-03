using Accounting.Api;
using Catalog.Api;
using Crm.Api;
using Identity.Api;
using Reference.Api;
using Reminders.Api;
using Host.Api.Pricing;

namespace Host.Api.DependencyInjection.ServiceRegistration.Web;

public static class ControllersExtensions
{
    public static IMvcBuilder AddModuleControllers(this IServiceCollection services)
        => services.AddControllers(options => options.Filters.Add<SewingPriceAutoPublicationFilter>())
            .AddApplicationPart(typeof(AccountingApiAssemblyMarker).Assembly)
            .AddApplicationPart(typeof(CatalogApiAssemblyMarker).Assembly)
            .AddApplicationPart(typeof(CrmApiAssemblyMarker).Assembly)
            .AddApplicationPart(typeof(IdentityApiAssemblyMarker).Assembly)
            .AddApplicationPart(typeof(ReferenceApiAssemblyMarker).Assembly)
            .AddApplicationPart(typeof(RemindersApiAssemblyMarker).Assembly);
}
