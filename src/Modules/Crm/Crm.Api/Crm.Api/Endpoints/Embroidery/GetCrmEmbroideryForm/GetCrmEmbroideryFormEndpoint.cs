using Crm.Application.Features.Embroidery.Queries.GetEmbroideryForm;
using Crm.Application.Features.Embroidery.Shared.Access;
using BuildingBlocks.Api.Errors;

namespace Crm.Api.Endpoints.Embroidery.GetCrmEmbroideryForm;

internal static class GetCrmEmbroideryFormEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("form", HandleAsync).WithName("getCrmEmbroideryForm");

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user, [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var actor = EmbroideryActorClaims.Read(user);
        if (actor is null) return Results.Forbid();
        return (await sender.Send(new GetEmbroideryFormQuery(actor), cancellationToken)).ToHttpResult();
    }
}
