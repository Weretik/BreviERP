using Crm.Application.Features.Embroidery.Commands.SaveEmbroideryCalculation;
using Crm.Application.Features.Embroidery.Shared;
using Crm.Application.Features.Embroidery.Shared.Access;
using BuildingBlocks.Api.Errors;

namespace Crm.Api.Endpoints.Embroidery.SaveCrmEmbroideryCalculation;

/// <summary>«Зберегти розрахунок»: the server prices the form with the current rates and freezes them.</summary>
internal static class SaveCrmEmbroideryCalculationEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("calculations", HandleAsync).WithName("saveCrmEmbroideryCalculation")
            .WithSummary("«Зберегти розрахунок»")
            .WithDescription("«Зберегти розрахунок». The server prices the form with the rates in force now, freezes those rates with the record and never recalculates it. An «Орієнтовна» price (`incomplete`) can be saved; a price that failed (`error`), an unknown or hidden client is 400. The name is trimmed and cut to 120 characters.")
            .Produces<EmbroiderySavedCalculationResponse>();

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
