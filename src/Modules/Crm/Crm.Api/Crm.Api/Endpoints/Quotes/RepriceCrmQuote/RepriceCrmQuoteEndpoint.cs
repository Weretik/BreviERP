using Crm.Application.Features.Quotes.Shared;
using Crm.Api.Access;
using Crm.Application.Features.Quotes.Commands.RepriceQuote;
using BuildingBlocks.Api.Errors;

namespace Crm.Api.Endpoints.Quotes.RepriceCrmQuote;

/// <summary>«Перерахувати за актуальними цінами»: preview with apply=false, then apply=true.</summary>
internal static class RepriceCrmQuoteEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("{id:long}/reprice", HandleAsync).WithName("repriceCrmQuote")
            .WithSummary("«Перерахувати за актуальними цінами»")
            .WithDescription("«Перерахувати за актуальними цінами». apply=false only lists the changes; apply=true replaces the Catalog prices of the listed rows. Manual PPE prices stay; rows the Catalog cannot price are listed in unavailable and keep their prices.")
            .RequireAccessAction(AccessActionAttribute.Edit)
            .Produces<QuoteRepriceReport>();

    private static async Task<IResult> HandleAsync(long id, RepriceQuoteRequest request, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken)
    {
        var actor = QuoteActorClaims.Read(user);
        if (actor is null) return Results.Forbid();
        return (await sender.Send(
            new RepriceQuoteCommand(actor, id, request.Version, request.Apply), cancellationToken)).ToHttpResult();
    }
}
