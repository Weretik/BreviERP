using Crm.Application.Features.Embroidery.Queries.CalculateEmbroidery;
using Crm.Application.Features.Embroidery.Shared;
using Crm.Application.Features.Embroidery.Shared.Access;

namespace Crm.Api.Endpoints.Embroidery.CalculateCrmEmbroidery;

/// <summary>Computes the price without saving anything; a Manager gets no cost.</summary>
internal static class CalculateCrmEmbroideryEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("calculate", HandleAsync).WithName("calculateCrmEmbroidery")
            .RequireAccessAction(AccessActionAttribute.View);

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user, CalculateEmbroideryInput input,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var actor = EmbroideryActorClaims.Read(user);
        if (actor is null) return Results.Forbid();
        return (await sender.Send(new CalculateEmbroideryQuery(actor, input), cancellationToken)).ToHttpResult();
    }
}
