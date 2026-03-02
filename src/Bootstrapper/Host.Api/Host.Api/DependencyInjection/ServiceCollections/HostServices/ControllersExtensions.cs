using Accounting.Api;
using Crm.Api;
using Reference.Api;

namespace Host.Api.DependencyInjection.ServiceCollections.HostServices;

public static class ControllersExtensions
{
    public static IMvcBuilder AddModuleControllers(this IServiceCollection services)
        => services.AddControllers()
            .AddApplicationPart(typeof(AccountingApiAssemblyMarker).Assembly)
            .AddApplicationPart(typeof(CrmApiAssemblyMarker).Assembly)
            .AddApplicationPart(typeof(ReferenceApiAssemblyMarker).Assembly);
}
