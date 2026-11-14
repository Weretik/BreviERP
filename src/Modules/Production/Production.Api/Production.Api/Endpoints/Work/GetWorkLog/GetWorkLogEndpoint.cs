using Production.Application.Features.Work.Queries.GetWorkLog;

namespace Production.Api.Endpoints.Work.GetWorkLog;

/// <summary>Records of a month (<c>YYYY-MM</c>), optionally of one worker or order.</summary>
internal static class GetWorkLogEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("work", HandleAsync).WithName("getWorkLog");

    private static async Task<IResult> HandleAsync(string month, ClaimsPrincipal user, [FromServices] ISender sender,
        int? employeeId = null, int? orderId = null, CancellationToken cancellationToken = default) =>
        user.Actor() is { } actor
            ? (await sender.Send(new GetWorkLogQuery(actor, month, employeeId, orderId),
                cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
