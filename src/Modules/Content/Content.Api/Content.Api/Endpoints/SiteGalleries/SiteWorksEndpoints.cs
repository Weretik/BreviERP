using Content.Api.Access;
using Content.Api.Endpoints.SiteGalleries.GetSiteWorks;
using Content.Api.Endpoints.SiteGalleries.SaveSiteWorks;
using Content.Api.Endpoints.SiteGalleries.TransferSiteWorks;

namespace Content.Api.Endpoints.SiteGalleries;

/// <summary>«Контент → Наші роботи» (CNT-011).</summary>
internal static class SiteWorksEndpoints
{
    public static IEndpointRouteBuilder MapSiteWorksEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/content/works").RequireAuthorization(ContentAccess.WorksPolicy)
            .WithTags("SiteWorks");
        GetSiteWorksEndpoint.Map(group);
        SaveSiteWorksEndpoint.Map(group);
        TransferSiteWorksEndpoint.Map(group);
        return app;
    }
}
