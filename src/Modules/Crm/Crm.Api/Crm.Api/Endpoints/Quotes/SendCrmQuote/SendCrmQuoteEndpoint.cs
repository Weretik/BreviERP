using Crm.Api.Access;
using Crm.Application.Features.Quotes.Commands.SendQuote;

namespace Crm.Api.Endpoints.Quotes.SendCrmQuote;

/// <summary>Sends the PDF of the saved КП from the signed-in person's Gmail; 409 carries the reason of a failure.</summary>
internal static class SendCrmQuoteEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("{id:long}/send", HandleAsync).WithName("sendCrmQuote")
            .RequireAccessAction(AccessActionAttribute.Edit);

    private static async Task<IResult> HandleAsync(long id, SendQuoteInput input, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken)
    {
        var actor = QuoteActorClaims.Read(user);
        if (actor is null) return Results.Forbid();
        return (await sender.Send(new SendQuoteCommand(actor, id, input), cancellationToken)).ToHttpResult();
    }
}
