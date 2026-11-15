using Production.Application.Features.Orders.Queries.GetProductionQuoteSources;
using BuildingBlocks.Api.Errors;

namespace Production.Api.Endpoints.Orders.GetProductionQuoteSources;

/// <summary>КП «під запит» the person may pass to the workshop.</summary>
internal static class GetProductionQuoteSourcesEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("quote-sources", HandleAsync).WithName("getProductionQuoteSources")
            .WithSummary("КП «під запит» (saved or sent) the person may pass to the workshop (Manager: own; Admin: all)")
            .Produces<IReadOnlyList<ProductionQuoteSourceResponse>>();

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user, [FromServices] ISender sender,
        string? search = null, CancellationToken cancellationToken = default) =>
        user.Actor() is { } actor
            ? (await sender.Send(new GetProductionQuoteSourcesQuery(actor, search), cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
