using Identity.Infrastructure.Integrations.AspNetIdentity;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Identity.Api.Auth;

namespace Identity.Api.DependencyInjection;

public static class IdentityCookieCsrfExtensions
{
    /// <summary>CSRF check for browser cookie sessions; the session cookie name comes from Identity's settings.</summary>
    public static IApplicationBuilder UseCookieCsrfProtection(this IApplicationBuilder app)
    {
        var sessionCookieName = app.ApplicationServices.GetRequiredService<IOptions<IdentityCookieSessionOptions>>().Value.CookieName;
        return app.UseMiddleware<CookieCsrfMiddleware>(sessionCookieName);
    }
}
