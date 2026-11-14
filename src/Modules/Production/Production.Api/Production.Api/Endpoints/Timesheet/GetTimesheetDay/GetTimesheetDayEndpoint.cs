using Production.Application.Features.Timesheet.Queries.GetTimesheetDay;

namespace Production.Api.Endpoints.Timesheet.GetTimesheetDay;

/// <summary>A day with the totals of its month.</summary>
internal static class GetTimesheetDayEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getTimesheetDay");

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user, [FromServices] ISender sender,
        DateOnly? date = null, CancellationToken cancellationToken = default) =>
        user.Actor() is { } actor
            ? (await sender.Send(new GetTimesheetDayQuery(actor, date ?? default), cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
