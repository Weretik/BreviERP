using Content.Application.Features.Catalog.Shared.Links;
using Content.Application.Features.Catalog.Queries.GetPublicCatalogProductSlugs;
using BuildingBlocks.Api.Errors;

namespace Content.Api.Endpoints.PublicCatalog.GetPublicCatalogProductSlugs;

internal static class GetPublicCatalogProductSlugsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("product-slugs", HandleAsync).WithName("getPublicCatalogProductSlugs")
            .WithSummary("Addresses of every product on the site, for the sitemap and for building pages ahead of time")
            .Produces<IReadOnlyList<PublicCatalogSlug>>();

    private static async Task<IResult> HandleAsync([FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetPublicCatalogProductSlugsQuery(), cancellationToken)).ToHttpResult();
}
