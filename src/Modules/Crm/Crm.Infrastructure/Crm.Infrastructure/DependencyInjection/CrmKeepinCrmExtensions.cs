using Crm.Domain.Customers.Identifiers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Crm.Application.Features.Customers.Commands.SyncKeepinCrmClients;
using Crm.Application.Abstractions.Customers;
using Crm.Infrastructure.Integrations.KeepinCrm;
using Crm.Infrastructure.Persistence.Customers;

namespace Crm.Infrastructure.DependencyInjection;

internal static class CrmKeepinCrmExtensions
{
    /// <summary>KeepinCRM client source, store and sync service; part of AddCrmInfrastructure.</summary>
    internal static IServiceCollection AddCrmKeepinCrmSync(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<KeepinCrmClientOptions>(configuration.GetSection(KeepinCrmClientOptions.Section));
        services.AddHttpClient<IKeepinCrmClientSource, KeepinCrmClientHttpSource>(client => client.Timeout = TimeSpan.FromSeconds(30));
        services.AddSingleton(provider => new KeepinCrmClientSyncSettings(
            new CrmUserId(provider.GetRequiredService<IOptions<KeepinCrmClientOptions>>().Value.ClientOwnerUserId)));
        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<ICustomerKeepinSyncStore, CustomerKeepinSyncStore>();
        services.AddScoped<KeepinCrmClientSync>();
        return services;
    }
}
