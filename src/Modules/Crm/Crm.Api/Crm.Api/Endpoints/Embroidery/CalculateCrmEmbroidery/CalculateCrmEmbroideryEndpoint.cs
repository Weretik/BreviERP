using Crm.Application.Features.Embroidery.Queries.CalculateEmbroidery;
using Crm.Application.Features.Embroidery.Shared;
using Crm.Application.Features.Embroidery.Shared.Access;
using BuildingBlocks.Api.Errors;

namespace Crm.Api.Endpoints.Embroidery.CalculateCrmEmbroidery;

/// <summary>Computes the price without saving anything; a Manager gets no cost.</summary>
internal static class CalculateCrmEmbroideryEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("calculate", HandleAsync).WithName("calculateCrmEmbroidery")
            .WithSummary("Computes the price and saves nothing")
            .WithDescription("Computes the price and saves nothing. A problem that stops the price (a blank wider than the roll, an unknown hoop, no stitch rate) comes back as `error` with status 200 and no prices. `cost` and money of the cut layout are null for a Manager.")
            .RequireAccessAction(AccessActionAttribute.View)
            .Produces<EmbroideryCalculationResponse>();

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user, CalculateEmbroideryInput input,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var actor = EmbroideryActorClaims.Read(user);
        if (actor is null) return Results.Forbid();
        return (await sender.Send(new CalculateEmbroideryQuery(actor, input), cancellationToken)).ToHttpResult();
    }
}
