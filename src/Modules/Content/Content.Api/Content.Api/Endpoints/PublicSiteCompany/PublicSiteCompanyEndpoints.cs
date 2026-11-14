using BuildingBlocks.Api.Caching;
using Content.Api.Endpoints.PublicSiteCompany.GetPublicSiteCompany;

namespace Content.Api.Endpoints.PublicSiteCompany;

/// <summary>
/// The company facts of the public site (CNT-009): read-only, without sign-in. Only what every visitor sees on the
/// site anyway: phones, emails, address, hours, managers, payment details and profile links.
/// </summary>
internal static class PublicSiteCompanyEndpoints
{
    /// <summary>How long browsers and the CDN may keep an answer, as before on the controller.</summary>
    public const int CacheSeconds = 300;

    public static IEndpointRouteBuilder MapPublicSiteCompanyEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/public/company").AllowAnonymous()
            .WithPublicResponseCache(CacheSeconds)
            .WithTags("PublicSiteCompany");
        GetPublicSiteCompanyEndpoint.Map(group);
        return app;
    }
}
