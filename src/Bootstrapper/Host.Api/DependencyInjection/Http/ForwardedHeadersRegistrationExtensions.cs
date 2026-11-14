using Microsoft.AspNetCore.HttpOverrides;


namespace Host.Api.DependencyInjection.Http;

public static class ForwardedHeadersRegistrationExtensions
{
    public static IServiceCollection AddForwardedHeadersRegistration(this IServiceCollection services)
    {
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                                       ForwardedHeaders.XForwardedProto |
                                       ForwardedHeaders.XForwardedHost;

            options.KnownIPNetworks.Clear();
            options.KnownProxies.Clear();
        });

        return services;
    }
}
