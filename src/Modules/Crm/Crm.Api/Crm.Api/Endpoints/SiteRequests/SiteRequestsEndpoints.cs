using Crm.Api.Access;

namespace Crm.Api.Endpoints.SiteRequests;

/// <summary>
/// «Менеджер сервісу › Заявки з сайту» (LEAD-002): requests of the public site. Everyone with the page sees them and
/// takes a free one; the owner or an Admin closes or frees it.
/// </summary>
internal static class SiteRequestsEndpoints
{
    public static IEndpointRouteBuilder MapSiteRequestsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/crm/site-requests").RequireAuthorization(CrmAccess.SiteRequests)
            .WithTags("SiteRequests");
        GetCrmSiteRequestsEndpoint.Map(group);
        ChangeCrmSiteRequestEndpoint.Map(group);
        return app;
    }
}
