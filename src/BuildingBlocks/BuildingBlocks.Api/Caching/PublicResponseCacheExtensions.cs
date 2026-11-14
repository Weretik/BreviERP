using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Api.Caching;

/// <summary>
/// The Minimal API form of MVC's <c>[ResponseCache(Duration = n, Location = ResponseCacheLocation.Any)]</c>: the same
/// <c>Cache-Control: public,max-age=n</c> header, set before the endpoint runs, so browsers and the CDN may keep the
/// public site's answers for <paramref name="seconds"/>.
/// </summary>
public static class PublicResponseCacheExtensions
{
    public static TBuilder WithPublicResponseCache<TBuilder>(this TBuilder builder, int seconds)
        where TBuilder : IEndpointConventionBuilder =>
        builder.AddEndpointFilter(async (context, next) =>
        {
            context.HttpContext.Response.Headers.CacheControl = $"public,max-age={seconds}";
            return await next(context);
        });
}
