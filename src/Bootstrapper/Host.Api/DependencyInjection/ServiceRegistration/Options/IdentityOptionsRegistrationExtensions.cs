using Identity.Api.Options;
using Identity.Infrastructure.Configuration;
using Identity.Infrastructure.Options;

namespace Host.Api.DependencyInjection.ServiceRegistration.Options;

public static class IdentityOptionsRegistrationExtensions
{
    public static IServiceCollection AddIdentityConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<AdminUserConfig>(
            configuration.GetSection("Identity:AdminUser"));

        services
            .AddOptions<SessionCookieOptions>()
            .Bind(configuration.GetSection(SessionCookieOptions.SectionName))
            .Validate(
                sessionCookieOptions =>
                    !string.IsNullOrWhiteSpace(sessionCookieOptions.RefreshTokenCookieName) &&
                    !string.IsNullOrWhiteSpace(sessionCookieOptions.RefreshTokenCookiePath) &&
                    sessionCookieOptions.RefreshTokenCookieTtlDays > 0 &&
                    !string.IsNullOrWhiteSpace(sessionCookieOptions.CsrfCookieName) &&
                    !string.IsNullOrWhiteSpace(sessionCookieOptions.CsrfHeaderName) &&
                    !string.IsNullOrWhiteSpace(sessionCookieOptions.CsrfCookiePath) &&
                    sessionCookieOptions.CsrfCookieTtlDays > 0,
                "Identity:SessionCookies configuration is invalid.");

        services
            .AddOptions<IdentitySessionPerformanceOptions>()
            .Bind(configuration.GetSection(IdentitySessionPerformanceOptions.SectionName))
            .Validate(
                performanceOptions => performanceOptions.SlowStepThresholdMs > 0,
                "Identity:SessionPerformance configuration is invalid.")
            .ValidateOnStart();

        services
            .AddOptions<IdentitySessionSecurityOptions>()
            .Bind(configuration.GetSection(IdentitySessionSecurityOptions.SectionName))
            .Validate(
                securityOptions =>
                    securityOptions.AccessTokenLifetimeMinutes is >= 5 and <= 60 &&
                    securityOptions.RefreshAbsoluteLifetimeDays is > 0 and <= 365 &&
                    securityOptions.RefreshIdleTimeoutDays is > 0 &&
                    securityOptions.RefreshIdleTimeoutDays <= securityOptions.RefreshAbsoluteLifetimeDays,
                "Identity:SessionSecurity configuration is invalid.")
            .ValidateOnStart();

        return services;
    }
}
