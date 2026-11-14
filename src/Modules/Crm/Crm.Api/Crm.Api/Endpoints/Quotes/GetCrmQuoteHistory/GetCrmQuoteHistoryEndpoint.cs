using Crm.Api.Access;
using Crm.Application.Features.Quotes.Queries.GetQuoteHistory;
using BuildingBlocks.Api.Errors;

namespace Crm.Api.Endpoints.Quotes.GetCrmQuoteHistory;

/// <summary>«Історія змін»: who created, saved, sent, repriced, passed, archived or copied the КП.</summary>
internal static class GetCrmQuoteHistoryEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("{id:long}/history", HandleAsync).WithName("getCrmQuoteHistory");

    private static async Task<IResult> HandleAsync(long id, ClaimsPrincipal user, [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var actor = QuoteActorClaims.Read(user);
        if (actor is null) return Results.Forbid();
        return (await sender.Send(new GetQuoteHistoryQuery(actor, id), cancellationToken)).ToHttpResult();
    }
}
