using Content.Api.Access;

namespace Content.Api.Endpoints.SiteCompany;

/// <summary>«Контент → Контакти та компанія» (CNT-009).</summary>
internal static class SiteCompanyEndpoints
{
    public static IEndpointRouteBuilder MapSiteCompanyEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/content/company").RequireAuthorization(ContentAccess.CompanyPolicy)
            .WithTags("SiteCompany");
        GetSiteCompanyEndpoint.Map(group);
        SaveSiteCompanyEndpoint.Map(group);
        return app;
    }
}
