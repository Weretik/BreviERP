using Crm.Api.Access;
using Crm.Application.Features.Quotes.Commands.SaveQuote;
using Crm.Application.Features.Quotes.Shared;
using BuildingBlocks.Api.Errors;

namespace Crm.Api.Endpoints.Quotes.SaveCrmQuote;

/// <summary>Saves the whole editor; 409 when someone saved a newer version.</summary>
internal static class SaveCrmQuoteEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("{id:long}", HandleAsync).WithName("saveCrmQuote")
            .WithSummary("Saves the whole editor content")
            .WithDescription("Saves the whole editor content. Saved rows keep their prices; only the shown flags and PPE manual prices change.")
            .Produces<QuoteResponse>();

    private static async Task<IResult> HandleAsync(long id, SaveQuoteInput input, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken)
    {
        var actor = QuoteActorClaims.Read(user);
        if (actor is null) return Results.Forbid();
        return (await sender.Send(new SaveQuoteCommand(actor, id, input), cancellationToken)).ToHttpResult();
    }
}
