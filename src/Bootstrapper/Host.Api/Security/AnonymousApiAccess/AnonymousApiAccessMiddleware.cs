using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Host.Api.Security.AnonymousApiAccess;

public sealed class AnonymousApiAccessMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, IAnonymousApiActorResolver actors)
    {
        // Already-public actions do not need an audit actor or an Identity database query.
        var endpoint = context.GetEndpoint();
        if (!context.Request.Path.StartsWithSegments("/api") ||
            endpoint?.Metadata.GetMetadata<IAuthorizeData>() is null)
        {
            await next(context);
            return;
        }

        var actor = await actors.ResolveAsync(context.RequestAborted);
        if (actor is null)
        {
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            await context.Response.WriteAsJsonAsync(new { code = "anonymous_api_actor_unavailable" },
                context.RequestAborted);
            return;
        }

        // Internal attribution only: never issue a bearer token or create a login session.
        context.User = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, actor.UserId.ToString(CultureInfo.InvariantCulture)),
            new Claim(ClaimTypes.Name, actor.Name),
            new Claim(ClaimTypes.Email, actor.Email),
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim(ClaimTypes.Role, "Manager"),
            new Claim(ClaimTypes.Role, "User")
        ], "TemporaryAnonymousApi", ClaimTypes.Name, ClaimTypes.Role));
        await next(context);
    }
}
