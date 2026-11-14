using Microsoft.Extensions.DependencyInjection.Extensions;
using Reference.Application.Abstractions;
using Reference.Application.Features.Supplier.Commands.SyncKeepinCrmSuppliers;
using Reference.Infrastructure.Integrations.KeepinCrm;

namespace Reference.Infrastructure.DependencyInjection;

public static class ReferenceKeepinCrmExtensions
{
    /// <summary>KeepinCRM supplier client and the sync service; used by Host.Api and the Host.Seed sync job.</summary>
    public static IServiceCollection AddReferenceKeepinCrmSync(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<KeepinCrmOptions>(configuration.GetSection(KeepinCrmOptions.Section));
        services.AddHttpClient<IKeepinCrmSupplierSource, KeepinCrmSupplierClient>(client => client.Timeout = TimeSpan.FromSeconds(30));
        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<KeepinCrmSupplierSync>();
        return services;
    }
}
