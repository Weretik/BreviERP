using Accounting.Api;
using Crm.Api;
using Identity.Api;
using Reference.Api;

namespace Host.Api.DependencyInjection.ServiceRegistration.Web;

public static class ControllersExtensions
{
    public static IMvcBuilder AddModuleControllers(this IServiceCollection services)
        => services.AddControllers()
            .AddApplicationPart(typeof(AccountingApiAssemblyMarker).Assembly)
            .AddApplicationPart(typeof(CrmApiAssemblyMarker).Assembly)
            .AddApplicationPart(typeof(IdentityApiAssemblyMarker).Assembly)
            .AddApplicationPart(typeof(ReferenceApiAssemblyMarker).Assembly);
}
