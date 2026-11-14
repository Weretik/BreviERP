
namespace Identity.Api.Cookies;

/// <summary>
/// Protects browser cookie sessions from cross-site requests: a request that carries the session cookie
/// and changes state must send the CSRF header equal to the CSRF cookie.
/// </summary>
public sealed class CookieCsrfMiddleware(
    RequestDelegate next,
    SessionCookieWriter cookies,
    string sessionCookieName)
{
    // Login must work even when a stale session cookie is still in the browser; SameSite=Lax covers it.
    private static readonly PathString CookieLoginPath = "/api/auth/cookie/login";

    public Task InvokeAsync(HttpContext context)
    {
        var request = context.Request;
        if (!request.Cookies.ContainsKey(sessionCookieName))
        {
            return next(context);
        }

        if (HttpMethods.IsGet(request.Method) || HttpMethods.IsHead(request.Method) || HttpMethods.IsOptions(request.Method))
        {
            // The CSRF cookie has a fixed lifetime while the session slides; reissue it when it is gone.
            cookies.EnsureCsrfToken(request, context.Response);
            return next(context);
        }

        if (request.Path.Equals(CookieLoginPath, StringComparison.OrdinalIgnoreCase) || cookies.HasValidCsrfToken(request))
        {
            return next(context);
        }

        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        return context.Response.WriteAsync("Invalid CSRF token.");
    }
}
