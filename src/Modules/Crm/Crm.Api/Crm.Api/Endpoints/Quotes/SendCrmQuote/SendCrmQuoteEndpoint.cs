using Crm.Application.Features.Quotes.Shared;
using Crm.Api.Access;
using Crm.Application.Features.Quotes.Commands.SendQuote;
using BuildingBlocks.Api.Errors;

namespace Crm.Api.Endpoints.Quotes.SendCrmQuote;

/// <summary>Sends the PDF of the saved КП from the signed-in person's Gmail; 409 carries the reason of a failure.</summary>
internal static class SendCrmQuoteEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("{id:long}/send", HandleAsync).WithName("sendCrmQuote")
            .WithSummary("Sends the PDF of the saved КП of exactly `version` from the signed-in person's Gmail")
            .WithDescription("Sends the PDF of the saved КП of exactly `version` from the signed-in person's Gmail. On success the КП becomes Sent with `lastSentAt` and its version grows; it can be edited and sent again. Any failure (no Gmail, Gmail refused, stale version) leaves the КП as it was and returns 409 with the reason.")
            .RequireAccessAction(AccessActionAttribute.Edit)
            .Produces<QuoteResponse>();

    private static async Task<IResult> HandleAsync(long id, SendQuoteInput input, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken)
    {
        var actor = QuoteActorClaims.Read(user);
        if (actor is null) return Results.Forbid();
        return (await sender.Send(new SendQuoteCommand(actor, id, input), cancellationToken)).ToHttpResult();
    }
}
