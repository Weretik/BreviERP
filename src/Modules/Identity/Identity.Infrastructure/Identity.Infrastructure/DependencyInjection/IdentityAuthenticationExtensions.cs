using Microsoft.AspNetCore.Authentication.Cookies;
using Identity.Infrastructure.Integrations.AspNetIdentity;

namespace Identity.Infrastructure.DependencyInjection;

/// <summary>
/// Registers the Identity session cookie, the only sign-in scheme of BreviERP.
/// </summary>
internal static class IdentityAuthenticationExtensions
{
    internal static IServiceCollection AddIdentityAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var cookieSessionOptions = configuration
            .GetSection(IdentityCookieSessionOptions.SectionName)
            .Get<IdentityCookieSessionOptions>() ?? new IdentityCookieSessionOptions();

        // All Identity cookie schemes, because SignInManager.SignOutAsync signs out of each of them.
        services
            .AddAuthentication(IdentityConstants.ApplicationScheme)
            .AddIdentityCookies();

        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.Name = cookieSessionOptions.CookieName;
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = cookieSessionOptions.RequireSecure
                ? CookieSecurePolicy.Always
                : CookieSecurePolicy.SameAsRequest;
            options.Cookie.IsEssential = true;
            options.ExpireTimeSpan = TimeSpan.FromDays(cookieSessionOptions.LifetimeDays);
            options.SlidingExpiration = true;
            // An API answers with status codes; Admin shows the login or "no access" page.
            options.Events.OnRedirectToLogin = context => WriteStatusAsync(context, StatusCodes.Status401Unauthorized);
            options.Events.OnRedirectToAccessDenied = context => WriteStatusAsync(context, StatusCodes.Status403Forbidden);
        });

        services.Configure<SecurityStampValidatorOptions>(options =>
            options.ValidationInterval = TimeSpan.FromMinutes(cookieSessionOptions.SecurityStampValidationMinutes));

        return services;
    }

    private static Task WriteStatusAsync(RedirectContext<CookieAuthenticationOptions> context, int statusCode)
    {
        context.Response.StatusCode = statusCode;
        return Task.CompletedTask;
    }
}
