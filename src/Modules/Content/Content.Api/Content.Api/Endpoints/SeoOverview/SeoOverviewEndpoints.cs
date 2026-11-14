using Content.Api.Access;

namespace Content.Api.Endpoints.SeoOverview;

/// <summary>«Контент → SEO-огляд»: what the site misses for search (CNT-008).</summary>
internal static class SeoOverviewEndpoints
{
    public static IEndpointRouteBuilder MapSeoOverviewEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/content/seo-overview").RequireAuthorization(ContentAccess.SeoOverviewPolicy)
            .WithTags("SeoOverview");
        GetSeoOverviewEndpoint.Map(group);
        return app;
    }
}
