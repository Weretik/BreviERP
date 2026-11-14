namespace Crm.Api.Endpoints.SiteRequests;

/// <summary>
/// The «Заявка» form of the public site (LEAD-002): anonymous, a phone and optionally a file up to 10 MB, limited
/// by the host's rate policy <see cref="SubmitPublicSiteRequestEndpoint.RatePolicy"/>.
/// </summary>
internal static class PublicSiteRequestsEndpoints
{
    public static IEndpointRouteBuilder MapPublicSiteRequestsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/public/site-requests").AllowAnonymous()
            .WithTags("PublicSiteRequests");
        SubmitPublicSiteRequestEndpoint.Map(group);
        return app;
    }
}
