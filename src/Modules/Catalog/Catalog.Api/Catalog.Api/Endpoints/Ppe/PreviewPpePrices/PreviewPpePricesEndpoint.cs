using Catalog.Application.Features.Pricing.Queries.PreviewPpePrices;
using BuildingBlocks.Api.Errors;

namespace Catalog.Api.Endpoints.Ppe.PreviewPpePrices;

/// <summary>Three PPE prices for a supplier price and rule before saving (page «Новий ЗІЗ», CAT-009).</summary>
internal static class PreviewPpePricesEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("preview", HandleAsync).WithName("previewPpePrices")
            .WithSummary("Preview PPE prices")
            .WithDescription("CatalogRead. Three prices for a supplier price and rule before anything is saved (page «Новий ЗІЗ», CAT-009). Same calculator as the saved card; a draft rule gives the MissingRule blocker.")
            .RequireAuthorization("CatalogRead")
            .Produces<PpePricePreview>();

    private static async Task<IResult> HandleAsync(string ruleCode, [FromServices] ISender sender,
        decimal? supplierPrice = null, CancellationToken cancellationToken = default) =>
        (await sender.Send(new PreviewPpePricesQuery(ruleCode, supplierPrice ?? default),
            cancellationToken)).ToHttpResult();
}
