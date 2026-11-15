using Content.Application.Features.Catalog.Shared.Links;
using Content.Application.Features.Catalog.Queries.GetPublicCatalogCategorySlugs;
using BuildingBlocks.Api.Errors;

namespace Content.Api.Endpoints.PublicCatalog.GetPublicCatalogCategorySlugs;

internal static class GetPublicCatalogCategorySlugsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("category-slugs", HandleAsync).WithName("getPublicCatalogCategorySlugs")
            .WithSummary("Addresses of every shown category, for the sitemap and for building pages ahead of time")
            .Produces<IReadOnlyList<PublicCatalogSlug>>();

    private static async Task<IResult> HandleAsync([FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetPublicCatalogCategorySlugsQuery(), cancellationToken)).ToHttpResult();
}
