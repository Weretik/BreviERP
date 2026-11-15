using Content.Application.Features.Catalog.Shared.Products;
using Content.Application;
using Content.Application.Features.Catalog.Queries.GetPublicCatalogProduct;
using BuildingBlocks.Api.Errors;

namespace Content.Api.Endpoints.PublicCatalog.GetPublicCatalogProduct;

internal static class GetPublicCatalogProductEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("products/{slug}", HandleAsync).WithName("getPublicCatalogProduct")
            .WithSummary("A product page by its address")
            .WithDescription("A product page by its address. A Russian request for a product without Russian texts gets the Ukrainian texts, and language says which came back.")
            .Produces<PublicCatalogProduct>();

    private static async Task<IResult> HandleAsync(string slug, [FromServices] ISender sender,
        string language = SiteLanguages.Ukrainian, CancellationToken cancellationToken = default) =>
        (await sender.Send(new GetPublicCatalogProductQuery(slug, language), cancellationToken)).ToHttpResult();
}
