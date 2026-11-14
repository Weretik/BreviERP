using Crm.Api.Access;
using Crm.Application.Features.Quotes.Commands.SaveQuoteIntroduction;
using Crm.Application.Features.Quotes.Shared.Introductions;
using BuildingBlocks.Api.Errors;

namespace Crm.Api.Endpoints.Quotes.SaveCrmQuoteIntroduction;

/// <summary>Saves the whole introduction; 409 when someone saved a newer version.</summary>
internal static class SaveCrmQuoteIntroductionEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("{id:long}", HandleAsync).WithName("saveCrmQuoteIntroduction");

    private static async Task<IResult> HandleAsync(long id, SaveQuoteIntroductionInput input, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken)
    {
        var actor = QuoteActorClaims.Read(user);
        if (actor is null) return Results.Forbid();
        return (await sender.Send(new SaveQuoteIntroductionCommand(actor, id, input),
            cancellationToken)).ToHttpResult();
    }
}
