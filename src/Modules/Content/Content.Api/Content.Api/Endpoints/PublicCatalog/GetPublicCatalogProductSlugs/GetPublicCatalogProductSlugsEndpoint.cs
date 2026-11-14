using Content.Application.Features.Catalog.Queries.GetPublicCatalogProductSlugs;

namespace Content.Api.Endpoints.PublicCatalog.GetPublicCatalogProductSlugs;

internal static class GetPublicCatalogProductSlugsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("product-slugs", HandleAsync).WithName("getPublicCatalogProductSlugs");

    private static async Task<IResult> HandleAsync([FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetPublicCatalogProductSlugsQuery(), cancellationToken)).ToHttpResult();
}
