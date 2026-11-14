using Content.Application;
using Content.Application.Features.Catalog.Queries.GetPublicCatalogCategory;
using BuildingBlocks.Api.Errors;

namespace Content.Api.Endpoints.PublicCatalog.GetPublicCatalogCategory;

internal static class GetPublicCatalogCategoryEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("categories/{slug}", HandleAsync).WithName("getPublicCatalogCategory");

    private static async Task<IResult> HandleAsync(string slug, [FromServices] ISender sender,
        string language = SiteLanguages.Ukrainian, CancellationToken cancellationToken = default) =>
        (await sender.Send(new GetPublicCatalogCategoryQuery(slug, language), cancellationToken)).ToHttpResult();
}
