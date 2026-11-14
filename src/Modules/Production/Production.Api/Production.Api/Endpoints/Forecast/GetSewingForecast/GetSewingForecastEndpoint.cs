using Production.Application.Features.Forecast.Queries.GetForecast;
using BuildingBlocks.Api.Errors;

namespace Production.Api.Endpoints.Forecast.GetSewingForecast;

internal static class GetSewingForecastEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getSewingForecast");

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        user.Actor() is { } actor
            ? (await sender.Send(new GetForecastQuery(actor), cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
