using Crm.Application.Features.Embroidery.Queries.GetEmbroideryCalculations;
using Crm.Application.Features.Embroidery.Shared.Access;
using BuildingBlocks.Api.Errors;

namespace Crm.Api.Endpoints.Embroidery.GetCrmEmbroideryCalculations;

/// <summary>«Збережені»: newest first; search by name, client or «ВШ-0042».</summary>
internal static class GetCrmEmbroideryCalculationsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("calculations", HandleAsync).WithName("getCrmEmbroideryCalculations");

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user, [FromServices] ISender sender,
        string? search = null, int page = 1,
        int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var actor = EmbroideryActorClaims.Read(user);
        if (actor is null) return Results.Forbid();
        return (await sender.Send(new GetEmbroideryCalculationsQuery(actor, search, page, pageSize),
            cancellationToken)).ToHttpResult();
    }
}
