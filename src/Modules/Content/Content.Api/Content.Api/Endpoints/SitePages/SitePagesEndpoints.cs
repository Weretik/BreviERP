using Content.Api.Access;

namespace Content.Api.Endpoints.SitePages;

/// <summary>«Контент → Сторінки сайту»: search texts of the fixed pages of the public site (CNT-007).</summary>
internal static class SitePagesEndpoints
{
    public static IEndpointRouteBuilder MapSitePagesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/content/site-pages").RequireAuthorization(ContentAccess.SitePagesPolicy)
            .WithTags("SitePages");
        GetSitePagesEndpoint.Map(group);
        SaveSitePageEndpoint.Map(group);
        return app;
    }
}
