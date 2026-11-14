using Content.Application.Features.Catalog.Queries.GetPublicCatalogCategorySlugs;

namespace Content.Api.Endpoints.PublicCatalog.GetPublicCatalogCategorySlugs;

internal static class GetPublicCatalogCategorySlugsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("category-slugs", HandleAsync).WithName("getPublicCatalogCategorySlugs");

    private static async Task<IResult> HandleAsync([FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetPublicCatalogCategorySlugsQuery(), cancellationToken)).ToHttpResult();
}
