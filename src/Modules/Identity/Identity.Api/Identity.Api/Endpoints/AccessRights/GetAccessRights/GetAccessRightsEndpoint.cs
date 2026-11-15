using Identity.Application.Features.Access.Queries.GetAccessRights;
using Identity.Api.Access;

namespace Identity.Api.Endpoints.AccessRights.GetAccessRights;

internal static class GetAccessRightsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getAccessRights")
            .WithSummary("Pages of «Права доступу» and every staff role with its rights")
            .WithDescription("Policy `CanManageUsers` (Admin; the director also holds Admin). Built-in roles first in a fixed order, then new roles by title. A role never saved shows its defaults with version 0.").Produces<AccessRightsResponse>();

    private static async Task<IResult> HandleAsync([FromServices] ISender sender, CancellationToken cancellationToken) =>
        Results.Ok(AccessRightsResponse.From((await sender.Send(new GetAccessRightsQuery(), cancellationToken)).Value));
}
