using Production.Application.Features.Reports.Queries.GetWorkshopReport;
using BuildingBlocks.Api.Errors;

namespace Production.Api.Endpoints.Reports.GetWorkshopReport;

/// <summary>The months <c>from</c>..<c>to</c> (<c>YYYY-MM</c>) and, with both, the months to compare with.</summary>
internal static class GetWorkshopReportEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getWorkshopReport")
            .WithSummary("«Звіти цеху» of the months from..to (at most 12) and, with both compare months, the months to compare with (Admin)")
            .Produces<WorkshopReportPairResponse>();

    private static async Task<IResult> HandleAsync(string from, string to, ClaimsPrincipal user,
        [FromServices] ISender sender, string? compareFrom = null, string? compareTo = null,
        CancellationToken cancellationToken = default) =>
        user.Actor() is { } actor
            ? (await sender.Send(
                new GetWorkshopReportQuery(actor, from, to, compareFrom, compareTo), cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
