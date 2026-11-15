using Production.Application.Features.Settings.Shared;
using Production.Application.Features.Settings.Commands.UpdateWorkshopSettings;
using BuildingBlocks.Api.Errors;

namespace Production.Api.Endpoints.Settings.UpdateWorkshopSettings;

internal static class UpdateWorkshopSettingsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("", HandleAsync).WithName("updateWorkshopSettings")
            .WithSummary("Choose the group and topic, turn sending on or off")
            .Produces<WorkshopSettingsResponse>();

    private static async Task<IResult> HandleAsync(WorkshopSettingsInput input, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        user.Actor() is { } actor
            ? (await sender.Send(new UpdateWorkshopSettingsCommand(actor, input), cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
