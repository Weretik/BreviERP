using Crm.Api.Access;
using Crm.Application.Features.Quotes.Commands.RepriceQuote;

namespace Crm.Api.Endpoints.Quotes.RepriceCrmQuote;

/// <summary>«Перерахувати за актуальними цінами»: preview with apply=false, then apply=true.</summary>
internal static class RepriceCrmQuoteEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("{id:long}/reprice", HandleAsync).WithName("repriceCrmQuote")
            .RequireAccessAction(AccessActionAttribute.Edit);

    private static async Task<IResult> HandleAsync(long id, RepriceQuoteRequest request, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken)
    {
        var actor = QuoteActorClaims.Read(user);
        if (actor is null) return Results.Forbid();
        return (await sender.Send(
            new RepriceQuoteCommand(actor, id, request.Version, request.Apply), cancellationToken)).ToHttpResult();
    }
}
