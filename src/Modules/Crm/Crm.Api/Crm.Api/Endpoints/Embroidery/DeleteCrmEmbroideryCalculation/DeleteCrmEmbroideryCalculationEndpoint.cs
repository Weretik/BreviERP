using Crm.Application.Features.Embroidery.Commands.DeleteEmbroideryCalculation;
using Crm.Application.Features.Embroidery.Shared.Access;
using BuildingBlocks.Api.Errors;

namespace Crm.Api.Endpoints.Embroidery.DeleteCrmEmbroideryCalculation;

internal static class DeleteCrmEmbroideryCalculationEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapDelete("calculations/{id:long}", HandleAsync).WithName("deleteCrmEmbroideryCalculation")
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user, long id, [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var actor = EmbroideryActorClaims.Read(user);
        if (actor is null) return Results.Forbid();
        var result = await sender.Send(new DeleteEmbroideryCalculationCommand(actor, id), cancellationToken);
        return result.IsSuccess ? Results.NoContent() : result.ToHttpResult();
    }
}
