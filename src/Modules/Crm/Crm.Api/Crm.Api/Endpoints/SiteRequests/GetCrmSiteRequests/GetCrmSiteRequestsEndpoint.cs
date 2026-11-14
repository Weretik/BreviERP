using Crm.Application.Features.SiteRequests.Queries.GetSiteRequests;
using BuildingBlocks.Api.Errors;

namespace Crm.Api.Endpoints.SiteRequests.GetCrmSiteRequests;

/// <param name="scope">`Open` (default), `Mine`, `Closed` or `All`.</param>
internal static class GetCrmSiteRequestsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getCrmSiteRequests");

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user, [FromServices] ISender sender,
        string? scope = null, int page = 1,
        int pageSize = 25, CancellationToken cancellationToken = default)
    {
        if (CrmActorClaims.UserId(user) is not { } userId) return Results.Forbid();
        return (await sender.Send(new GetSiteRequestsQuery(userId, scope, page, pageSize),
            cancellationToken)).ToHttpResult();
    }
}
