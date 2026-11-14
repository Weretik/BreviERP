using Crm.Api.Access;
using Crm.Application.Features.Quotes.Queries.GetQuote;
using BuildingBlocks.Api.Errors;

namespace Crm.Api.Endpoints.Quotes.GetCrmQuote;

internal static class GetCrmQuoteEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("{id:long}", HandleAsync).WithName("getCrmQuote");

    private static async Task<IResult> HandleAsync(long id, ClaimsPrincipal user, [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var actor = QuoteActorClaims.Read(user);
        if (actor is null) return Results.Forbid();
        return (await sender.Send(new GetQuoteQuery(actor, id), cancellationToken)).ToHttpResult();
    }
}
