using Identity.Api.Endpoints.CookieAuth.LoginCookieSession;
using Identity.Api.Endpoints.CookieAuth.LogoutCookieSession;

namespace Identity.Api.Endpoints.CookieAuth;

/// <summary>Browser sign-in for Admin: an HttpOnly session cookie plus a readable CSRF cookie.</summary>
internal static class CookieAuthEndpoints
{
    public static IEndpointRouteBuilder MapCookieAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/auth/cookie").WithTags("CookieAuth");
        LoginCookieSessionEndpoint.Map(group);
        LogoutCookieSessionEndpoint.Map(group);
        return app;
    }
}
