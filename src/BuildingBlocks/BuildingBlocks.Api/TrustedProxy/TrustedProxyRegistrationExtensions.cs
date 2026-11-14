using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Api.TrustedProxy;

public static class TrustedProxyRegistrationExtensions
{
    public static IServiceCollection AddTrustedProxyRegistration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<TrustedProxyOptions>()
            .Bind(configuration.GetSection(TrustedProxyOptions.SectionName))
            .Validate(
                o => !string.IsNullOrWhiteSpace(o.ClientIpHeaderName) &&
                     !string.IsNullOrWhiteSpace(o.SecretHeaderName),
                "TrustedProxy configuration is invalid.")
            .ValidateOnStart();

        return services;
    }

    public static IApplicationBuilder UseTrustedProxyClientIp(this IApplicationBuilder app)
        => app.UseMiddleware<TrustedProxyClientIpMiddleware>();
}
