using Crm.Application.Features.Embroidery.Queries.GetEmbroiderySettings;
using Crm.Application.Features.Embroidery.Shared.Access;

namespace Crm.Api.Endpoints.Embroidery.GetCrmEmbroiderySettings;

internal static class GetCrmEmbroiderySettingsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("settings", HandleAsync).WithName("getCrmEmbroiderySettings")
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user, [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var actor = EmbroideryActorClaims.Read(user);
        if (actor is null) return Results.Forbid();
        return (await sender.Send(new GetEmbroiderySettingsQuery(actor), cancellationToken)).ToHttpResult();
    }
}
