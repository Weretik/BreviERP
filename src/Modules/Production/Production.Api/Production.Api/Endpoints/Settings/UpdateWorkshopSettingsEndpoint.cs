using Production.Application.Features.Settings.Commands.UpdateWorkshopSettings;

namespace Production.Api.Endpoints.Settings;

internal static class UpdateWorkshopSettingsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("", HandleAsync).WithName("updateWorkshopSettings");

    private static async Task<IResult> HandleAsync(WorkshopSettingsInput input, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        user.Actor() is { } actor
            ? (await sender.Send(new UpdateWorkshopSettingsCommand(actor, input), cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
