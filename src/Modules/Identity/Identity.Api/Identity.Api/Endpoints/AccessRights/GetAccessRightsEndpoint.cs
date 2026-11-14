using Identity.Application.Features.Access.Queries.GetAccessRights;
using Identity.Api.Access;

namespace Identity.Api.Endpoints.AccessRights;

internal static class GetAccessRightsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getAccessRights").Produces<AccessRightsResponse>();

    private static async Task<IResult> HandleAsync([FromServices] ISender sender, CancellationToken cancellationToken) =>
        Results.Ok(AccessRightsResponse.From((await sender.Send(new GetAccessRightsQuery(), cancellationToken)).Value));
}
