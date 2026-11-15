using Crm.Application.Features.Embroidery.Commands.SaveEmbroiderySettings;
using Crm.Application.Features.Embroidery.Shared;
using Crm.Application.Features.Embroidery.Shared.Access;
using BuildingBlocks.Api.Errors;

namespace Crm.Api.Endpoints.Embroidery.SaveCrmEmbroiderySettings;

/// <summary>Saves the whole page; 409 when someone saved a newer version.</summary>
internal static class SaveCrmEmbroiderySettingsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("settings", HandleAsync).WithName("saveCrmEmbroiderySettings")
            .WithSummary("Saves the whole page")
            .WithDescription("Saves the whole page. `version` is the one read; a stale version is 409.")
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" })
            .Produces<EmbroiderySettingsResponse>();

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user, SaveEmbroiderySettingsInput input,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var actor = EmbroideryActorClaims.Read(user);
        if (actor is null) return Results.Forbid();
        return (await sender.Send(new SaveEmbroiderySettingsCommand(actor, input), cancellationToken)).ToHttpResult();
    }
}
