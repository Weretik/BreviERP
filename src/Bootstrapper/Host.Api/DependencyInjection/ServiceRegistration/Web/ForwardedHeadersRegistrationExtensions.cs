using Microsoft.AspNetCore.HttpOverrides;

namespace Host.Api.DependencyInjection.ServiceRegistration.Web;

public static class ForwardedHeadersRegistrationExtensions
{
    public static IServiceCollection AddForwardedHeadersRegistration(this IServiceCollection services)
    {
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                                       ForwardedHeaders.XForwardedProto |
                                       ForwardedHeaders.XForwardedHost;

            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        });

        return services;
    }
}
