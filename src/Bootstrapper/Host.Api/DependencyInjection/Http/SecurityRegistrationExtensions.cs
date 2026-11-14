using BuildingBlocks.Api.TrustedProxy;
using Identity.Api.DependencyInjection;


namespace Host.Api.DependencyInjection.Http;

public static class SecurityRegistrationsExtensions
{
    public static IServiceCollection AddSecurityRegistrations(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddForwardedHeadersRegistration();
        services.AddTrustedProxyRegistration(configuration);
        services.AddCorsService(configuration);
        services.AddRateLimitingServices();
        services.AddAuthorizationPolicies();

        return services;
    }
}
