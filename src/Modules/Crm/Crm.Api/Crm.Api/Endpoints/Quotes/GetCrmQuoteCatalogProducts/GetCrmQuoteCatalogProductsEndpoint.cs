using Crm.Application.Features.Quotes.Shared;
using Crm.Application.Features.Quotes.Queries.GetQuoteCatalogProducts;
using BuildingBlocks.Api.Errors;

namespace Crm.Api.Endpoints.Quotes.GetCrmQuoteCatalogProducts;

/// <summary>Catalog products by id, with their fabric variants and photo (at most 20).</summary>
internal static class GetCrmQuoteCatalogProductsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("products", HandleAsync).WithName("getCrmQuoteCatalogProducts")
            .WithSummary("Current Catalog data of chosen products (a saved introduction or a template); unknown ids are left out")
            .Produces<IReadOnlyList<QuoteCatalogProductResponse>>();

    private static async Task<IResult> HandleAsync([FromQuery] int[] ids, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetQuoteCatalogProductsQuery(ids), cancellationToken)).ToHttpResult();
}
