using Crm.Application.Features.Embroidery.Queries.GetEmbroideryCalculation;
using Crm.Application.Features.Embroidery.Shared.Access;

namespace Crm.Api.Endpoints.Embroidery.GetCrmEmbroideryCalculation;

/// <summary>One saved calculation as it was saved; a Manager gets no cost and no rates.</summary>
internal static class GetCrmEmbroideryCalculationEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("calculations/{id:long}", HandleAsync).WithName("getCrmEmbroideryCalculation");

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user, long id, [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var actor = EmbroideryActorClaims.Read(user);
        if (actor is null) return Results.Forbid();
        return (await sender.Send(new GetEmbroideryCalculationQuery(actor, id), cancellationToken)).ToHttpResult();
    }
}
