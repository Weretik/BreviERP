using Production.Application.Features.Timesheet.Commands.SaveTimesheetDay;
using BuildingBlocks.Api.Errors;

namespace Production.Api.Endpoints.Timesheet.SaveTimesheetDay;

/// <summary>Saves the changed marks of a day.</summary>
internal static class SaveTimesheetDayEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("", HandleAsync).WithName("saveTimesheetDay");

    private static async Task<IResult> HandleAsync(SaveTimesheetDayInput input, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        user.Actor() is { } actor
            ? (await sender.Send(new SaveTimesheetDayCommand(actor, input), cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
