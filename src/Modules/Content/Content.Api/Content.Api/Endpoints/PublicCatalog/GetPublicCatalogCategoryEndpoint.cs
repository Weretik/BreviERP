using Content.Application;
using Content.Application.Features.Catalog.Queries.GetPublicCatalogCategory;

namespace Content.Api.Endpoints.PublicCatalog;

internal static class GetPublicCatalogCategoryEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("categories/{slug}", HandleAsync).WithName("getPublicCatalogCategory");

    private static async Task<IResult> HandleAsync(string slug, [FromServices] ISender sender,
        string language = SiteLanguages.Ukrainian, CancellationToken cancellationToken = default) =>
        (await sender.Send(new GetPublicCatalogCategoryQuery(slug, language), cancellationToken)).ToHttpResult();
}
