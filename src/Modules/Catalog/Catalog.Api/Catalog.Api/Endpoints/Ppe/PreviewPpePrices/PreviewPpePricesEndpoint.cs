using Catalog.Application.Features.Pricing.Queries.PreviewPpePrices;

namespace Catalog.Api.Endpoints.Ppe.PreviewPpePrices;

/// <summary>Three PPE prices for a supplier price and rule before saving (page «Новий ЗІЗ», CAT-009).</summary>
internal static class PreviewPpePricesEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("preview", HandleAsync).WithName("previewPpePrices")
            .RequireAuthorization("CatalogRead");

    private static async Task<IResult> HandleAsync(string ruleCode, [FromServices] ISender sender,
        decimal? supplierPrice = null, CancellationToken cancellationToken = default) =>
        (await sender.Send(new PreviewPpePricesQuery(ruleCode, supplierPrice ?? default),
            cancellationToken)).ToHttpResult();
}
