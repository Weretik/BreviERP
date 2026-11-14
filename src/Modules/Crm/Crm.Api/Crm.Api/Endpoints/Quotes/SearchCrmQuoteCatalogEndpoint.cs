using Crm.Application.Features.Quotes.Queries.SearchQuoteCatalog;

namespace Crm.Api.Endpoints.Quotes;

/// <param name="kind">`Sewing` or `Ppe`.</param>
/// <param name="season">`Summer` or `Winter`: only sewing models of that season.</param>
internal static class SearchCrmQuoteCatalogEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("catalog", HandleAsync).WithName("searchCrmQuoteCatalog");

    private static async Task<IResult> HandleAsync(string kind, [FromServices] ISender sender, string? search = null,
        string? season = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default) =>
        (await sender.Send(new SearchQuoteCatalogQuery(kind, search, page, pageSize, season),
            cancellationToken)).ToHttpResult();
}
