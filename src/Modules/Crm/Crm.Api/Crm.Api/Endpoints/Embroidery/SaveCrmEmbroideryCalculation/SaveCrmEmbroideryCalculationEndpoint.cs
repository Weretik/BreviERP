using Crm.Application.Features.Embroidery.Commands.SaveEmbroideryCalculation;
using Crm.Application.Features.Embroidery.Shared;
using Crm.Application.Features.Embroidery.Shared.Access;
using BuildingBlocks.Api.Errors;

namespace Crm.Api.Endpoints.Embroidery.SaveCrmEmbroideryCalculation;

/// <summary>«Зберегти розрахунок»: the server prices the form with the current rates and freezes them.</summary>
internal static class SaveCrmEmbroideryCalculationEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("calculations", HandleAsync).WithName("saveCrmEmbroideryCalculation");

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user, SaveEmbroideryCalculationInput input,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var actor = EmbroideryActorClaims.Read(user);
        if (actor is null) return Results.Forbid();
        return (await sender.Send(new SaveEmbroideryCalculationCommand(actor, input),
            cancellationToken)).ToHttpResult();
    }
}
