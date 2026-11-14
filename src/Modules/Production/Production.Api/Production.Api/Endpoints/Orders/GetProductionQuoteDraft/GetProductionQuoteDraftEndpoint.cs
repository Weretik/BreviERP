using Production.Application.Features.Orders.Queries.GetProductionQuoteDraft;
using BuildingBlocks.Api.Errors;

namespace Production.Api.Endpoints.Orders.GetProductionQuoteDraft;

/// <summary>The КП for the order form.</summary>
internal static class GetProductionQuoteDraftEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("quote-sources/{quoteId:long}", HandleAsync).WithName("getProductionQuoteDraft");

    private static async Task<IResult> HandleAsync(long quoteId, ClaimsPrincipal user, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        user.Actor() is { } actor
            ? (await sender.Send(new GetProductionQuoteDraftQuery(actor, quoteId), cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
