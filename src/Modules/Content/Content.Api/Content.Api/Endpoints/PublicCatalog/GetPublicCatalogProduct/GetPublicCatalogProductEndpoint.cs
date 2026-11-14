using Content.Application;
using Content.Application.Features.Catalog.Queries.GetPublicCatalogProduct;
using BuildingBlocks.Api.Errors;

namespace Content.Api.Endpoints.PublicCatalog.GetPublicCatalogProduct;

internal static class GetPublicCatalogProductEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("products/{slug}", HandleAsync).WithName("getPublicCatalogProduct");

    private static async Task<IResult> HandleAsync(string slug, [FromServices] ISender sender,
        string language = SiteLanguages.Ukrainian, CancellationToken cancellationToken = default) =>
        (await sender.Send(new GetPublicCatalogProductQuery(slug, language), cancellationToken)).ToHttpResult();
}
