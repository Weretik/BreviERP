using Production.Application.Features.Settings.Queries.GetWorkshopSettings;

namespace Production.Api.Endpoints.Settings;

internal static class GetWorkshopSettingsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getWorkshopSettings");

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        user.Actor() is { } actor
            ? (await sender.Send(new GetWorkshopSettingsQuery(actor), cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
