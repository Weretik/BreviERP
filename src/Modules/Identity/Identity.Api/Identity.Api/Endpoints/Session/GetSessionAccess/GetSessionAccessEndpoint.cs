using Identity.Application.Features.Access.Queries.GetMyAccess;
using Identity.Api.Access;

namespace Identity.Api.Endpoints.Session.GetSessionAccess;

/// <summary>What the signed-in user may do, by page of «Права доступу»: the admin hides menu items and buttons by it; the
/// server checks the same rights on every request.</summary>
internal static class GetSessionAccessEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("access", HandleAsync).WithName("getSessionAccess")
            .WithSummary("What the signed-in user may do")
            .WithDescription("Rights by page of «Права доступу» ([ACC-001](../../specs/identity/002-access-rights/README.md)); the admin hides menu items and buttons by them, the server checks the same rights.").Produces<MyAccessResponse>();

    private static async Task<IResult> HandleAsync([FromServices] ISender sender, CancellationToken cancellationToken) =>
        Results.Ok(MyAccessResponse.From((await sender.Send(new GetMyAccessQuery(), cancellationToken)).Value));
}
