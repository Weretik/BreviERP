using Crm.Application.Features.Quotes.Queries.GetQuoteCatalogProducts;

namespace Crm.Api.Endpoints.Quotes;

/// <summary>Catalog products by id, with their fabric variants and photo (at most 20).</summary>
internal static class GetCrmQuoteCatalogProductsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("products", HandleAsync).WithName("getCrmQuoteCatalogProducts");

    private static async Task<IResult> HandleAsync([FromQuery] int[] ids, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetQuoteCatalogProductsQuery(ids), cancellationToken)).ToHttpResult();
}
