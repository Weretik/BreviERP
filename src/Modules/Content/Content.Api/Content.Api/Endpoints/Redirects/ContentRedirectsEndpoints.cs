using Content.Api.Access;

namespace Content.Api.Endpoints.Redirects;

/// <summary>«Контент → Переадресації»: 301 redirects of the public site (CNT-005).</summary>
internal static class ContentRedirectsEndpoints
{
    public static IEndpointRouteBuilder MapContentRedirectsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/content/redirects").RequireAuthorization(ContentAccess.RedirectsPolicy)
            .WithTags("ContentRedirects");
        GetSiteRedirectsEndpoint.Map(group);
        CreateSiteRedirectEndpoint.Map(group);
        SaveSiteRedirectEndpoint.Map(group);
        ImportOldSiteRedirectsEndpoint.Map(group);
        DeleteSiteRedirectEndpoint.Map(group);
        return app;
    }
}
