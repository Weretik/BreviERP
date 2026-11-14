using Production.Application.Features.Forecast.Commands.SaveForecastQueue;

namespace Production.Api.Endpoints.Forecast;

internal static class SaveSewingForecastQueueEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("queue", HandleAsync).WithName("saveSewingForecastQueue");

    private static async Task<IResult> HandleAsync(SaveForecastQueueInput input, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        user.Actor() is { } actor
            ? (await sender.Send(new SaveForecastQueueCommand(actor, input), cancellationToken)).ToNoContentHttpResult()
            : Results.Forbid();
}
