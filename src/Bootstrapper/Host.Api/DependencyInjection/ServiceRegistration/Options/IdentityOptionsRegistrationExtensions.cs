using Identity.Api.Cookies;
using Identity.Api.Options;
using Identity.Infrastructure.Options;

namespace Host.Api.DependencyInjection.ServiceRegistration.Options;

public static class IdentityOptionsRegistrationExtensions
{
    public static IServiceCollection AddIdentityConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<SessionCookieOptions>()
            .Bind(configuration.GetSection(SessionCookieOptions.SectionName))
            .Validate(
                o => !string.IsNullOrWhiteSpace(o.CsrfCookieName) &&
                     !string.IsNullOrWhiteSpace(o.CsrfHeaderName) &&
                     !string.IsNullOrWhiteSpace(o.CsrfCookiePath) &&
                     o.CsrfCookieTtlDays > 0,
                "Identity:SessionCookies configuration is invalid.")
            .ValidateOnStart();

        services.AddSingleton<SessionCookieWriter>();

        services
            .AddOptions<IdentityCookieSessionOptions>()
            .Bind(configuration.GetSection(IdentityCookieSessionOptions.SectionName))
            .Validate(
                o => !string.IsNullOrWhiteSpace(o.CookieName) &&
                     o.LifetimeDays is > 0 and <= 90 &&
                     o.SecurityStampValidationMinutes is > 0 and <= 60 &&
                     (o.RequireSecure || !o.CookieName.StartsWith("__Host-", StringComparison.Ordinal)),
                "Identity:CookieSession configuration is invalid.")
            .ValidateOnStart();

        return services;
    }
}
