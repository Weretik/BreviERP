using Identity.Application.Features.Auth.Queries.SessionMe;
using Identity.Api.Endpoints.Shared;
using BuildingBlocks.Api.Errors;

namespace Identity.Api.Endpoints.Session.GetSessionMe;

internal static class GetSessionMeEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("me", HandleAsync).WithName("getSessionMe").Produces<SessionMeResponse>();

    private static async Task<IResult> HandleAsync([FromServices] ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new SessionMeQuery(), cancellationToken);
        return result.Status != Ardalis.Result.ResultStatus.Ok || result.Value is null
            ? result.ToHttpResult()
            : Results.Ok(SessionMeResponse.From(result.Value));
    }
}
